using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace AtelierSystem;

public partial class MasterAppointmentsPage : UserControl
{
    private string currentFilter = "all";

    public MasterAppointmentsPage()
    {
        InitializeComponent();
        LoadAppointments();
    }

    private void LoadAppointments()
    {
        appointmentsPanel.Children.Clear();

        var master = App.dataBaseContext.Masters
            .FirstOrDefault(m => m.UserId == CurrentUser.currentUser.Id);

        if (master == null)
        {
            var errorText = new TextBlock
            {
                Text = "Мастер не найден.",
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            appointmentsPanel.Children.Add(errorText);
            return;
        }

        var appointments = App.dataBaseContext.Appointments
            .Where(a => a.MasterId == master.Id)
            .OrderBy(a => a.AppointmentTime)
            .ToList();

        if (currentFilter == "pending")
            appointments = appointments.Where(a => a.Status == "pending").ToList();
        else if (currentFilter == "confirmed")
            appointments = appointments.Where(a => a.Status == "confirmed").ToList();
        else if (currentFilter == "completed")
            appointments = appointments.Where(a => a.Status == "completed").ToList();

        if (!appointments.Any())
        {
            var emptyText = new TextBlock
            {
                Text = "Нет записей.",
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };
            appointmentsPanel.Children.Add(emptyText);
            return;
        }

        foreach (var appointment in appointments)
        {
            var user = App.dataBaseContext.Users.FirstOrDefault(u => u.Id == appointment.UserId);
            var service = App.dataBaseContext.Services.FirstOrDefault(s => s.Id == appointment.ServiceId);

            var card = CreateAppointmentCard(appointment, user, service);
            appointmentsPanel.Children.Add(card);
        }
    }

    private Border CreateAppointmentCard(Appointment appointment, User user, Service service)
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

        // Клиент
        var clientPanel = new StackPanel { Spacing = 5 };
        clientPanel.Children.Add(new TextBlock
        {
            Text = "Клиент:",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        clientPanel.Children.Add(new TextBlock { Text = user?.FullName ?? "Не указан" });
        Grid.SetColumn(clientPanel, 0);
        grid.Children.Add(clientPanel);

        // Услуга
        var servicePanel = new StackPanel { Spacing = 5 };
        servicePanel.Children.Add(new TextBlock
        {
            Text = "Услуга:",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        servicePanel.Children.Add(new TextBlock { Text = service?.Name ?? "Не указана" });
        servicePanel.Children.Add(new TextBlock { Text = $"Цена: {service?.Price:F2} ₽" });
        Grid.SetColumn(servicePanel, 1);
        grid.Children.Add(servicePanel);

        // Дата и статус
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
            Text = $"Очередь: {appointment.QueueNumber}"
        });
        datePanel.Children.Add(new TextBlock
        {
            Text = $"Статус: {GetStatusText(appointment.Status)}"
        });
        Grid.SetColumn(datePanel, 2);
        grid.Children.Add(datePanel);

        // Кнопки действий
        var actionPanel = new StackPanel { Spacing = 10, Margin = new Avalonia.Thickness(10, 0, 0, 0) };

        if (appointment.Status == "pending")
        {
            var confirmBtn = new Button
            {
                Content = "Подтвердить",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = appointment
            };
            confirmBtn.Click += confirmBtn_Click;
            actionPanel.Children.Add(confirmBtn);
        }

        if (appointment.Status == "confirmed")
        {
            var completeBtn = new Button
            {
                Content = "Выполнено",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = appointment
            };
            completeBtn.Click += completeBtn_Click;
            actionPanel.Children.Add(completeBtn);
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

    private async void confirmBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Appointment appointment)
        {
            appointment.Status = "confirmed";
            App.dataBaseContext.SaveChanges();

            await ShowMessageBox("Успешно", "Запись подтверждена.", Icon.Success);
            LoadAppointments();
        }
    }

    private async void completeBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Appointment appointment)
        {
            appointment.Status = "completed";
            App.dataBaseContext.SaveChanges();

            await ShowMessageBox("Успешно", "Запись отмечена как выполненная.", Icon.Success);
            LoadAppointments();
        }
    }

    private async System.Threading.Tasks.Task ShowMessageBox(string title, string message, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
        var parent = this.VisualRoot as Window;
        await box.ShowWindowDialogAsync(parent);
    }

    private void allFilterBtn_Click(object? sender, RoutedEventArgs e)
    {
        currentFilter = "all";
        LoadAppointments();
    }

    private void pendingFilterBtn_Click(object? sender, RoutedEventArgs e)
    {
        currentFilter = "pending";
        LoadAppointments();
    }

    private void confirmedFilterBtn_Click(object? sender, RoutedEventArgs e)
    {
        currentFilter = "confirmed";
        LoadAppointments();
    }

    private void completedFilterBtn_Click(object? sender, RoutedEventArgs e)
    {
        currentFilter = "completed";
        LoadAppointments();
    }

    private void refreshBtn_Click(object? sender, RoutedEventArgs e)
    {
        LoadAppointments();
    }
}