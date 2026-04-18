using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace AtelierSystem;

public partial class QualificationRequestsPage : UserControl
{
    public QualificationRequestsPage()
    {
        InitializeComponent();
        LoadRequests();
    }

    private void LoadRequests()
    {
        requestsPanel.Children.Clear();

        var requests = App.dataBaseContext.QualificationRequests
            .OrderByDescending(r => r.Status == "pending")
            .ThenBy(r => r.CreatedAt)
            .ToList();

        if (!requests.Any())
        {
            var emptyText = new TextBlock
            {
                Text = "Нет заявок.",
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };
            requestsPanel.Children.Add(emptyText);
            return;
        }

        foreach (var request in requests)
        {
            var master = App.dataBaseContext.Masters.FirstOrDefault(m => m.Id == request.MasterId);
            var masterUser = master != null ? App.dataBaseContext.Users.FirstOrDefault(u => u.Id == master.UserId) : null;

            var card = CreateRequestCard(request, master, masterUser);
            requestsPanel.Children.Add(card);
        }
    }

    private Border CreateRequestCard(QualificationRequest request, Master master, User masterUser)
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

        var masterPanel = new StackPanel { Spacing = 5 };
        masterPanel.Children.Add(new TextBlock
        {
            Text = "Мастер:",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        masterPanel.Children.Add(new TextBlock { Text = masterUser?.FullName ?? "Не указан" });
        masterPanel.Children.Add(new TextBlock { Text = $"Текущий уровень: {master?.QualificationLevel}" });
        Grid.SetColumn(masterPanel, 0);
        grid.Children.Add(masterPanel);

        var levelPanel = new StackPanel { Spacing = 5 };
        levelPanel.Children.Add(new TextBlock
        {
            Text = "Запрашиваемый уровень:",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        levelPanel.Children.Add(new TextBlock { Text = request.RequestedLevel });
        Grid.SetColumn(levelPanel, 1);
        grid.Children.Add(levelPanel);

        var datePanel = new StackPanel { Spacing = 5 };
        datePanel.Children.Add(new TextBlock
        {
            Text = "Дата заявки:",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        datePanel.Children.Add(new TextBlock
        {
            Text = request.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "Дата не указана"
        });
        datePanel.Children.Add(new TextBlock
        {
            Text = $"Статус: {GetStatusText(request.Status)}",
            Foreground = GetStatusColor(request.Status)
        });
        Grid.SetColumn(datePanel, 2);
        grid.Children.Add(datePanel);

        var actionPanel = new StackPanel { Spacing = 5 };

        if (request.Status == "pending")
        {
            var approveBtn = new Button
            {
                Content = "Одобрить",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = request
            };
            approveBtn.Click += approveBtn_Click;
            actionPanel.Children.Add(approveBtn);

            var rejectBtn = new Button
            {
                Content = "Отклонить",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = request
            };
            rejectBtn.Click += rejectBtn_Click;
            actionPanel.Children.Add(rejectBtn);
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
            "pending" => "На рассмотрении",
            "approved" => "Одобрено",
            "rejected" => "Отклонено",
            _ => status
        };
    }

    private Avalonia.Media.IBrush GetStatusColor(string status)
    {
        return status switch
        {
            "pending" => Avalonia.Media.Brushes.Orange,
            "approved" => Avalonia.Media.Brushes.Green,
            "rejected" => Avalonia.Media.Brushes.Red,
            _ => Avalonia.Media.Brushes.Black
        };
    }

    private async void approveBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is QualificationRequest request)
        {
            request.Status = "approved";
            request.ProcessedAt = DateTime.Now;

            var master = App.dataBaseContext.Masters.FirstOrDefault(m => m.Id == request.MasterId);
            if (master != null)
            {
                master.QualificationLevel = request.RequestedLevel;
            }

            App.dataBaseContext.SaveChanges();

            await ShowMessageBox("Успешно", "Заявка одобрена. Квалификация мастера повышена.", Icon.Success);
            LoadRequests();
        }
    }

    private async void rejectBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is QualificationRequest request)
        {
            request.Status = "rejected";
            request.ProcessedAt = DateTime.Now;
            App.dataBaseContext.SaveChanges();

            await ShowMessageBox("Успешно", "Заявка отклонена.", Icon.Info);
            LoadRequests();
        }
    }

    private void refreshBtn_Click(object? sender, RoutedEventArgs e)
    {
        LoadRequests();
    }

    private async System.Threading.Tasks.Task ShowMessageBox(string title, string message, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
        var parent = this.VisualRoot as Window;
        await box.ShowWindowDialogAsync(parent);
    }
}