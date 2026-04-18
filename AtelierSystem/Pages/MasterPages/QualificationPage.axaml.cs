using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierSystem;

public partial class QualificationPage : UserControl
{
    private Master currentMaster;
    private List<string> allLevels = new List<string>
    {
        "Начинающий",
        "Мастер",
        "Старший мастер",
        "Ведущий мастер",
        "Эксперт"
    };

    public QualificationPage()
    {
        InitializeComponent();
        LoadMasterData();
        LoadRequestHistory();
    }

    private void LoadMasterData()
    {
        currentMaster = App.dataBaseContext.Masters
            .FirstOrDefault(m => m.UserId == CurrentUser.currentUser.Id);

        if (currentMaster != null)
        {
            currentLevelBox.Text = currentMaster.QualificationLevel;
            LoadAvailableLevels();
        }
    }

    private void LoadAvailableLevels()
    {
        levelComboBox.Items.Clear();

        int currentLevelIndex = allLevels.IndexOf(currentMaster.QualificationLevel);

        if (currentLevelIndex == -1)
        {
            foreach (var level in allLevels)
            {
                levelComboBox.Items.Add(level);
            }
        }
        else
        {
            for (int i = currentLevelIndex + 1; i < allLevels.Count; i++)
            {
                levelComboBox.Items.Add(allLevels[i]);
            }
        }

        if (levelComboBox.Items.Count > 0)
        {
            levelComboBox.SelectedIndex = 0;
        }
    }

    private void LoadRequestHistory()
    {
        requestsPanel.Children.Clear();

        if (currentMaster == null) return;

        var requests = App.dataBaseContext.QualificationRequests
            .Where(r => r.MasterId == currentMaster.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        if (!requests.Any())
        {
            var emptyText = new TextBlock
            {
                Text = "История заявок пуста.",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray
            };
            requestsPanel.Children.Add(emptyText);
            return;
        }

        foreach (var request in requests)
        {
            var border = new Border
            {
                BorderBrush = Avalonia.Media.Brushes.LightGray,
                BorderThickness = new Avalonia.Thickness(1),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var levelText = new TextBlock { Text = $"Уровень: {request.RequestedLevel}" };
            Grid.SetColumn(levelText, 0);
            grid.Children.Add(levelText);

            var statusText = new TextBlock
            {
                Text = $"Статус: {GetStatusText(request.Status)}",
                Foreground = GetStatusColor(request.Status)
            };
            Grid.SetColumn(statusText, 1);
            grid.Children.Add(statusText);

            var dateText = new TextBlock { Text = $"Дата: {request.CreatedAt?.ToString("dd.MM.yyyy") ?? "Не указана"}" };
            Grid.SetColumn(dateText, 2);
            grid.Children.Add(dateText);

            border.Child = grid;
            requestsPanel.Children.Add(border);
        }
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

    private async void sendRequestBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (levelComboBox.Items.Count == 0)
            {
                await ShowMessageBox("Информация", "Вы уже имеете максимальный уровень квалификации.", Icon.Info);
                return;
            }

            if (levelComboBox.SelectedIndex == -1)
            {
                await ShowMessageBox("Ошибка", "Выберите уровень для запроса.", Icon.Warning);
                return;
            }

            string requestedLevel = levelComboBox.SelectedItem?.ToString() ?? "";

            var pendingRequest = App.dataBaseContext.QualificationRequests
                .FirstOrDefault(r => r.MasterId == currentMaster.Id && r.Status == "pending");

            if (pendingRequest != null)
            {
                await ShowMessageBox("Ошибка", "У вас уже есть заявка на рассмотрении.", Icon.Warning);
                return;
            }

            var request = new QualificationRequest()
            {
                MasterId = currentMaster.Id,
                RequestedLevel = requestedLevel,
                Status = "pending",
                CreatedAt = DateTime.Now
            };

            App.dataBaseContext.QualificationRequests.Add(request);
            App.dataBaseContext.SaveChanges();

            await ShowMessageBox("Успешно", "Заявка на повышение квалификации отправлена.", Icon.Success);

            LoadRequestHistory();
        }
        catch (Exception ex)
        {
            await ShowMessageBox("Ошибка", $"Произошла ошибка: {ex.Message}", Icon.Error);
        }
    }

    private async System.Threading.Tasks.Task ShowMessageBox(string title, string message, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
        var parent = this.VisualRoot as Window;
        await box.ShowWindowDialogAsync(parent);
    }
}