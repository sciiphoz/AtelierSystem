using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Linq;

namespace AtelierSystem;

public partial class ModeratorServicesPage : UserControl
{
    public ModeratorServicesPage()
    {
        InitializeComponent();
        LoadServices();
    }

    private void LoadServices()
    {
        servicesPanel.Children.Clear();

        var services = App.dataBaseContext.Services.ToList();

        foreach (var service in services)
        {
            service.Collection = App.dataBaseContext.Collections
                .FirstOrDefault(c => c.Id == service.CollectionId);
            service.Category = App.dataBaseContext.ServiceCategories
                .FirstOrDefault(c => c.Id == service.CategoryId);

            var card = CreateServiceCard(service);
            servicesPanel.Children.Add(card);
        }
    }

    private Border CreateServiceCard(Service service)
    {
        var border = new Border
        {
            BorderBrush = Avalonia.Media.Brushes.LightGray,
            BorderThickness = new Avalonia.Thickness(1),
            Padding = new Avalonia.Thickness(15),
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

        // Название и описание
        var namePanel = new StackPanel { Spacing = 5 };
        namePanel.Children.Add(new TextBlock
        {
            Text = service.Name,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 14
        });
        namePanel.Children.Add(new TextBlock
        {
            Text = service.Description ?? "Нет описания",
            FontSize = 12,
            Foreground = Avalonia.Media.Brushes.Gray
        });
        Grid.SetColumn(namePanel, 0);
        grid.Children.Add(namePanel);

        // Цена
        var priceText = new TextBlock
        {
            Text = $"{service.Price:F2} ₽",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(priceText, 1);
        grid.Children.Add(priceText);

        // Категория и коллекция
        var categoryPanel = new StackPanel { Spacing = 3 };
        categoryPanel.Children.Add(new TextBlock { Text = service.Category?.Name ?? "Без категории" });
        categoryPanel.Children.Add(new TextBlock
        {
            Text = service.Collection?.Name ?? "Без коллекции",
            FontSize = 11,
            Foreground = Avalonia.Media.Brushes.Gray
        });
        Grid.SetColumn(categoryPanel, 2);
        grid.Children.Add(categoryPanel);

        // Кнопки действий
        var actionPanel = new StackPanel { Spacing = 5 };

        var editBtn = new Button
        {
            Content = "Изменить",
            Padding = new Avalonia.Thickness(10, 5),
            Background = Avalonia.Media.Brushes.White,
            BorderBrush = Avalonia.Media.Brushes.Black,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Tag = service
        };
        editBtn.Click += editBtn_Click;
        actionPanel.Children.Add(editBtn);

        Grid.SetColumn(actionPanel, 3);
        grid.Children.Add(actionPanel);

        border.Child = grid;
        return border;
    }

    private void editBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Service service)
        {
            var editPage = new EditServicePage(service);

            if (this.VisualRoot is ModeratorWindow window)
            {
                window.mainContentControl.Content = editPage;
            }
        }
    }

    private void addServiceBtn_Click(object? sender, RoutedEventArgs e)
    {
        var addPage = new AddServicePage();

        if (this.VisualRoot is ModeratorWindow window)
        {
            window.mainContentControl.Content = addPage;
        }
    }

    private void refreshBtn_Click(object? sender, RoutedEventArgs e)
    {
        LoadServices();
    }
}