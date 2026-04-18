using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace AtelierSystem;

public partial class MyAppointmentsPage : UserControl
{
    public MyAppointmentsPage()
    {
        InitializeComponent();
        LoadAppointments();
    }

    private void LoadAppointments()
    {
        appointmentsPanel.Children.Clear();

        var appointments = App.dataBaseContext.Appointments
            .Where(a => a.UserId == CurrentUser.currentUser.Id)
            .OrderByDescending(a => a.CreatedAt)  // От новых к старым
            .ToList();

        if (!appointments.Any())
        {
            var emptyText = new TextBlock
            {
                Text = "У вас пока нет записей.",
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };
            appointmentsPanel.Children.Add(emptyText);
            return;
        }

        foreach (var appointment in appointments)
        {
            var service = App.dataBaseContext.Services
                .FirstOrDefault(s => s.Id == appointment.ServiceId);

            var master = App.dataBaseContext.Masters
                .FirstOrDefault(m => m.Id == appointment.MasterId);

            var masterUser = master != null
                ? App.dataBaseContext.Users.FirstOrDefault(u => u.Id == master.UserId)
                : null;

            var appointmentCard = CreateAppointmentCard(appointment, service, masterUser, master);
            appointmentsPanel.Children.Add(appointmentCard);
        }
    }

    private Border CreateAppointmentCard(Appointment appointment, Service service, User masterUser, Master master)
    {
        var border = new Border
        {
            BorderBrush = Avalonia.Media.Brushes.LightGray,
            BorderThickness = new Avalonia.Thickness(1),
            Padding = new Avalonia.Thickness(15),
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

        var servicePanel = new StackPanel { Spacing = 5 };
        servicePanel.Children.Add(new TextBlock
        {
            Text = "Услуга:",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        servicePanel.Children.Add(new TextBlock
        {
            Text = service?.Name ?? "Не указана"
        });
        Grid.SetColumn(servicePanel, 0);
        grid.Children.Add(servicePanel);

        var masterPanel = new StackPanel { Spacing = 5 };
        masterPanel.Children.Add(new TextBlock
        {
            Text = "Мастер:",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        masterPanel.Children.Add(new TextBlock
        {
            Text = masterUser?.FullName ?? "Не указан"
        });
        masterPanel.Children.Add(new TextBlock
        {
            Text = master?.QualificationLevel ?? ""
        });
        Grid.SetColumn(masterPanel, 1);
        grid.Children.Add(masterPanel);

        var datePanel = new StackPanel { Spacing = 5 };
        datePanel.Children.Add(new TextBlock
        {
            Text = "Дата:",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        datePanel.Children.Add(new TextBlock
        {
            Text = appointment.AppointmentTime.ToString("dd.MM.yyyy HH:mm")
        });
        datePanel.Children.Add(new TextBlock
        {
            Text = $"Статус: {GetStatusText(appointment.Status)}"
        });
        datePanel.Children.Add(new TextBlock
        {
            Text = $"Очередь: {appointment.QueueNumber}"
        });
        Grid.SetColumn(datePanel, 2);
        grid.Children.Add(datePanel);

        var actionPanel = new StackPanel { Spacing = 10, Margin = new Avalonia.Thickness(10, 0, 0, 0) };

        if (appointment.Status == "completed")
        {
            var reviewBtn = new Button
            {
                Content = "Оставить отзыв",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = appointment
            };
            reviewBtn.Click += reviewBtn_Click;
            actionPanel.Children.Add(reviewBtn);
        }

        if (appointment.Status == "pending")
        {
            var cancelBtn = new Button
            {
                Content = "Отменить",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = appointment
            };
            cancelBtn.Click += cancelBtn_Click;
            actionPanel.Children.Add(cancelBtn);
        }

        Grid.SetColumn(actionPanel, 3);
        grid.Children.Add(actionPanel);

        border.Child = grid;
        return border;
    }

    private string GetStatusText(string status)
    {
        return status switch
        {
            "pending" => "Ожидание",
            "confirmed" => "Подтверждено",
            "completed" => "Выполнено",
            "cancelled" => "Отменено",
            _ => status
        };
    }

    private async void reviewBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Appointment appointment)
        {
            var parent = this.VisualRoot as Window;
            if (parent is UserWindow UserWindow)
            {
                UserWindow.mainContentControl.Content = new ReviewPage(appointment);
            }
        }
    }

    private async void cancelBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Appointment appointment)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Подтверждение",
                "Вы действительно хотите отменить запись? Деньги будут возвращены на баланс.",
                ButtonEnum.YesNo,
                Icon.Question);

            var parent = this.VisualRoot as Window;
            var result = await box.ShowWindowDialogAsync(parent);

            if (result == ButtonResult.Yes)
            {
                var service = App.dataBaseContext.Services.FirstOrDefault(s => s.Id == appointment.ServiceId);

                if (service != null)
                {
                    var user = App.dataBaseContext.Users.FirstOrDefault(u => u.Id == CurrentUser.currentUser.Id);
                    if (user != null)
                    {
                        user.Balance += service.Price;
                        CurrentUser.currentUser.Balance = user.Balance;
                    }
                }

                appointment.Status = "cancelled";
                App.dataBaseContext.SaveChanges();

                UpdateQueueNumbers(appointment.MasterId);

                if (this.VisualRoot is UserWindow userWindow)
                {
                    userWindow.UpdateUserInfo();
                }

                LoadAppointments();

                var successBox = MessageBoxManager.GetMessageBoxStandard(
                    "Успешно",
                    $"Запись отменена. Средства возвращены на баланс.",
                    ButtonEnum.Ok,
                    Icon.Success);

                await successBox.ShowWindowDialogAsync(parent);
            }
        }
    }

    private void backBtn_Click(object? sender, RoutedEventArgs e)
    {
        var parent = this.VisualRoot as Window;
        if (parent is UserWindow UserWindow)
        {
            UserWindow.mainContentControl.Content = new ServicesPage();
        }
    }

    private void UpdateQueueNumbers(int masterId)
    {
        var appointments = App.dataBaseContext.Appointments
            .Where(a => a.MasterId == masterId && a.Status != "cancelled")
            .OrderBy(a => a.AppointmentTime)
            .ToList();

        int queueNumber = 1;
        foreach (var appointment in appointments)
        {
            appointment.QueueNumber = queueNumber;
            queueNumber++;
        }

        App.dataBaseContext.SaveChanges();
    }
}