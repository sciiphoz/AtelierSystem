using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using System.Collections.Generic;
using System.Linq;

namespace AtelierSystem;

public partial class MasterServicesPage : UserControl
{
    private List<Master> masters;
    private Master selectedMaster;

    public MasterServicesPage()
    {
        InitializeComponent();
        LoadMasters();
    }

    private void LoadMasters()
    {
        masters = App.dataBaseContext.Masters.ToList();

        foreach (var master in masters)
        {
            var user = App.dataBaseContext.Users.FirstOrDefault(u => u.Id == master.UserId);
            masterComboBox.Items.Add($"{user?.FullName} ({master.QualificationLevel})");
        }

        if (masterComboBox.Items.Count > 0)
        {
            masterComboBox.SelectedIndex = 0;
        }
    }

    private void masterComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (masterComboBox.SelectedIndex >= 0)
        {
            selectedMaster = masters[masterComboBox.SelectedIndex];
            LoadServices();
        }
    }

    private void LoadServices()
    {
        if (selectedMaster == null) return;

        LoadAvailableServices();
        LoadMasterServices();
    }

    private void LoadAvailableServices()
    {
        availableServicesPanel.Children.Clear();

        var masterServiceIds = App.dataBaseContext.MasterServices
            .Where(ms => ms.MasterId == selectedMaster.Id)
            .Select(ms => ms.ServiceId)
            .ToList();

        var availableServices = App.dataBaseContext.Services
            .Where(s => !masterServiceIds.Contains(s.Id))
            .ToList();

        foreach (var service in availableServices)
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
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            var nameText = new TextBlock
            {
                Text = service.Name,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            var addBtn = new Button
            {
                Content = "← Добавить",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = service
            };
            addBtn.Click += addBtn_Click;
            Grid.SetColumn(addBtn, 1);
            grid.Children.Add(addBtn);

            border.Child = grid;
            availableServicesPanel.Children.Add(border);
        }

        if (!availableServices.Any())
        {
            var emptyText = new TextBlock
            {
                Text = "Нет доступных услуг",
                Foreground = Avalonia.Media.Brushes.Gray,
                Margin = new Avalonia.Thickness(0, 10, 0, 0)
            };
            availableServicesPanel.Children.Add(emptyText);
        }
    }

    private void LoadMasterServices()
    {
        masterServicesPanel.Children.Clear();

        var masterServices = App.dataBaseContext.MasterServices
            .Where(ms => ms.MasterId == selectedMaster.Id)
            .ToList();

        foreach (var masterService in masterServices)
        {
            var service = App.dataBaseContext.Services.FirstOrDefault(s => s.Id == masterService.ServiceId);
            if (service == null) continue;

            var border = new Border
            {
                BorderBrush = Avalonia.Media.Brushes.LightGray,
                BorderThickness = new Avalonia.Thickness(1),
                Padding = new Avalonia.Thickness(10),
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            var nameText = new TextBlock
            {
                Text = service.Name,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);

            var removeBtn = new Button
            {
                Content = "Убрать →",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = masterService
            };
            removeBtn.Click += removeBtn_Click;
            Grid.SetColumn(removeBtn, 1);
            grid.Children.Add(removeBtn);

            border.Child = grid;
            masterServicesPanel.Children.Add(border);
        }

        if (!masterServices.Any())
        {
            var emptyText = new TextBlock
            {
                Text = "У мастера пока нет услуг",
                Foreground = Avalonia.Media.Brushes.Gray,
                Margin = new Avalonia.Thickness(0, 10, 0, 0)
            };
            masterServicesPanel.Children.Add(emptyText);
        }
    }

    private void addBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Service service)
        {
            var masterService = new MasterService()
            {
                MasterId = selectedMaster.Id,
                ServiceId = service.Id
            };

            App.dataBaseContext.MasterServices.Add(masterService);
            App.dataBaseContext.SaveChanges();

            LoadServices();
        }
    }

    private void removeBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MasterService masterService)
        {
            App.dataBaseContext.MasterServices.Remove(masterService);
            App.dataBaseContext.SaveChanges();

            LoadServices();
        }
    }
}