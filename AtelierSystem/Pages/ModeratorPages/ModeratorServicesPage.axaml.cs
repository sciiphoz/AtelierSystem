using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AtelierSystem.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierSystem;

public partial class ModeratorServicesPage : UserControl
{
    private List<Service> allServices;
    private List<Service> filteredServices;
    private int currentPage = 1;
    private int itemsPerPage = 3;
    private string currentCategory = "Кастомизация";
    private string currentSort = "По умолчанию";
    private string currentCollection = "Все коллекции";

    public ModeratorServicesPage()
    {
        InitializeComponent();
        LoadLogo();

        LoadServices();
        UpdateServicesDisplay();

        sortComboBox.SelectedIndex = 0;
        collectionFilterBox.SelectedIndex = 0;
    }

    private void LoadLogo()
    {
        try
        {
            var uri = new Uri("avares://AtelierSystem/Assets/Icon/Logo.png");
            logoImage.Source = new Bitmap(AssetLoader.Open(uri));
        }
        catch
        {
            // Если логотип не найден, оставляем пустым
        }
    }

    private void LoadServices()
    {
        allServices = App.dataBaseContext.Services.ToList();

        foreach (var service in allServices)
        {
            service.Collection = App.dataBaseContext.Collections
                .FirstOrDefault(c => c.Id == service.CollectionId);
            service.Category = App.dataBaseContext.ServiceCategories
                .FirstOrDefault(c => c.Id == service.CategoryId);
        }

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredServices = allServices.Where(s =>
            s.Category?.Name == currentCategory
        ).ToList();

        ApplyCollectionFilter();
        ApplySearchFilter();
        ApplySorting();

        currentPage = 1;
        UpdateServicesDisplay();
    }

    private void ApplyCollectionFilter()
    {
        if (currentCollection != "Все коллекции")
        {
            filteredServices = filteredServices.Where(s =>
                s.Collection != null && s.Collection.Name == currentCollection
            ).ToList();
        }
    }

    private void ApplySearchFilter()
    {
        string searchText = searchBox.Text?.Trim() ?? string.Empty;

        if (!string.IsNullOrEmpty(searchText))
        {
            filteredServices = filteredServices.Where(s =>
                s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                (s.Description != null && s.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }
    }

    private void ApplySorting()
    {
        switch (currentSort)
        {
            case "По алфавиту (А-Я)":
                filteredServices = filteredServices.OrderBy(s => s.Name).ToList();
                break;
            case "По алфавиту (Я-А)":
                filteredServices = filteredServices.OrderByDescending(s => s.Name).ToList();
                break;
            case "По цене (возр.)":
                filteredServices = filteredServices.OrderBy(s => s.Price).ToList();
                break;
            case "По цене (убыв.)":
                filteredServices = filteredServices.OrderByDescending(s => s.Price).ToList();
                break;
        }
    }

    private void UpdateServicesDisplay()
    {
        servicesPanel.Children.Clear();

        var pageServices = filteredServices
            .Skip((currentPage - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToList();

        foreach (var service in pageServices)
        {
            var serviceCard = CreateServiceCard(service);
            servicesPanel.Children.Add(serviceCard);
        }

        UpdatePaginationInfo();
    }

    private Border CreateServiceCard(Service service)
    {
        var border = new Border
        {
            BorderBrush = Avalonia.Media.Brushes.LightGray,
            BorderThickness = new Avalonia.Thickness(1),
            Padding = new Avalonia.Thickness(10),
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var photoBorder = new Border
        {
            Width = 150,
            Height = 150,
            Background = Avalonia.Media.Brushes.LightGray
        };

        var imageUri = GetServiceImageUri(service);
        if (!string.IsNullOrEmpty(imageUri))
        {
            try
            {
                var uri = new Uri(imageUri);
                var image = new Image
                {
                    Source = new Bitmap(AssetLoader.Open(uri)),
                    Width = 150,
                    Height = 150,
                    Stretch = Avalonia.Media.Stretch.UniformToFill
                };
                photoBorder.Child = image;
            }
            catch
            {
                var photoText = new TextBlock
                {
                    Text = "Фото",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                photoBorder.Child = photoText;
            }
        }
        else
        {
            var photoText = new TextBlock
            {
                Text = "Фото",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            photoBorder.Child = photoText;
        }

        Grid.SetColumn(photoBorder, 0);
        grid.Children.Add(photoBorder);

        var infoPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(10, 0, 0, 0),
            Spacing = 5
        };

        infoPanel.Children.Add(new TextBlock
        {
            Text = service.Name,
            FontSize = 16,
            FontWeight = Avalonia.Media.FontWeight.Bold
        });

        infoPanel.Children.Add(new TextBlock
        {
            Text = service.Description ?? "Описание отсутствует",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        infoPanel.Children.Add(new TextBlock
        {
            Text = $"Цена: {service.Price:F2} ₽",
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });

        infoPanel.Children.Add(new TextBlock
        {
            Text = $"Коллекция: {service.Collection?.Name ?? "Не указана"}"
        });

        infoPanel.Children.Add(new TextBlock
        {
            Text = $"Категория: {service.Category?.Name ?? "Не указана"}"
        });

        var masterInfo = GetMasterInfo(service.Id);
        if (!string.IsNullOrEmpty(masterInfo))
        {
            infoPanel.Children.Add(new TextBlock
            {
                Text = masterInfo,
                FontSize = 13,
                Foreground = Avalonia.Media.Brushes.DimGray
            });
        }

        infoPanel.Children.Add(new TextBlock
        {
            Text = $"Изменено: {service.UpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "Не изменялось"}",
            FontSize = 11,
            Foreground = Avalonia.Media.Brushes.Gray
        });

        var editBtn = new Button
        {
            Content = "Редактировать",
            Margin = new Avalonia.Thickness(0, 10, 0, 0),
            Padding = new Avalonia.Thickness(15, 5),
            Background = Avalonia.Media.Brushes.White,
            BorderBrush = Avalonia.Media.Brushes.Black,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Tag = service
        };
        editBtn.Click += editBtn_Click;
        infoPanel.Children.Add(editBtn);

        Grid.SetColumn(infoPanel, 1);
        grid.Children.Add(infoPanel);

        border.Child = grid;
        return border;
    }

    private string GetMasterInfo(int serviceId)
    {
        var masterServices = App.dataBaseContext.MasterServices
            .Where(ms => ms.ServiceId == serviceId)
            .ToList();

        if (!masterServices.Any())
            return string.Empty;

        var masterNames = new List<string>();

        foreach (var ms in masterServices)
        {
            var master = App.dataBaseContext.Masters.FirstOrDefault(m => m.Id == ms.MasterId);
            if (master != null)
            {
                var user = App.dataBaseContext.Users.FirstOrDefault(u => u.Id == master.UserId);
                if (user != null)
                {
                    masterNames.Add($"{user.FullName} ({master.QualificationLevel})");
                }
            }
        }

        return masterNames.Count == 1
            ? $"Мастер: {masterNames[0]}"
            : $"Мастера: {string.Join(", ", masterNames)}";
    }

    private string GetServiceImageUri(Service service)
    {
        string category = service.Category?.Name ?? "";
        int imageNumber;

        if (service.Id > 10)
            imageNumber = service.Id - 10;
        else
            imageNumber = service.Id;

        if (category == "Кастомизация")
            return $"avares://AtelierSystem/Assets/Data/Custom/Pr{imageNumber}.jpg";
        else if (category == "Создание косплея")
            return $"avares://AtelierSystem/Assets/Data/Cosplay/KL{imageNumber}.jpg";

        return string.Empty;
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

    private void UpdatePaginationInfo()
    {
        int totalPages = (int)Math.Ceiling((double)filteredServices.Count / itemsPerPage);
        int startItem = filteredServices.Count == 0 ? 0 : (currentPage - 1) * itemsPerPage + 1;
        int endItem = Math.Min(currentPage * itemsPerPage, filteredServices.Count);

        pageInfoText.Text = $"{startItem}-{endItem} из {filteredServices.Count}";

        prevPageBtn.IsEnabled = currentPage > 1;
        nextPageBtn.IsEnabled = currentPage < totalPages;
    }

    private void customBtn_Click(object? sender, RoutedEventArgs e)
    {
        currentCategory = "Кастомизация";
        ApplyFilters();
    }

    private void cosplayBtn_Click(object? sender, RoutedEventArgs e)
    {
        currentCategory = "Создание косплея";
        ApplyFilters();
    }

    private void searchBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void sortComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sortComboBox.SelectedItem is ComboBoxItem item)
        {
            currentSort = item.Content?.ToString() ?? "По умолчанию";
            ApplyFilters();
        }
    }

    private void collectionFilterBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (collectionFilterBox.SelectedItem is ComboBoxItem item)
        {
            currentCollection = item.Content?.ToString() ?? "Все коллекции";
            ApplyFilters();
        }
    }

    private void resetFilterBtn_Click(object? sender, RoutedEventArgs e)
    {
        searchBox.Text = string.Empty;
        sortComboBox.SelectedIndex = 0;
        collectionFilterBox.SelectedIndex = 0;
        ApplyFilters();
    }

    private void prevPageBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            UpdateServicesDisplay();
        }
    }

    private void nextPageBtn_Click(object? sender, RoutedEventArgs e)
    {
        int totalPages = (int)Math.Ceiling((double)filteredServices.Count / itemsPerPage);
        if (currentPage < totalPages)
        {
            currentPage++;
            UpdateServicesDisplay();
        }
    }
}