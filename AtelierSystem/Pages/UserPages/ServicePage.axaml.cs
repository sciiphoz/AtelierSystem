using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierSystem;

public partial class ServicesPage : UserControl
{
    private List<Service> allServices;
    private List<Service> filteredServices;
    private int currentPage = 1;
    private int itemsPerPage = 3;
    private string currentCategory = "Кастомизация";

    public ServicesPage()
    {
        InitializeComponent();
        LoadServices();
        UpdateServicesDisplay();
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

        ApplyCollectionFilters();
        ApplySearchFilter();

        currentPage = 1;
        UpdateServicesDisplay();
    }

    private void ApplyCollectionFilters()
    {
        var selectedCollections = new List<string>();

        if (animeFilter.IsChecked == true) selectedCollections.Add("Аниме");
        if (newYearFilter.IsChecked == true) selectedCollections.Add("Новый год");
        if (halloweenFilter.IsChecked == true) selectedCollections.Add("Хэллоуин");
        if (cyberpunkFilter.IsChecked == true) selectedCollections.Add("Киберпанк");
        if (noirFilter.IsChecked == true) selectedCollections.Add("Нуар");

        if (selectedCollections.Any())
        {
            filteredServices = filteredServices.Where(s =>
                s.Collection != null && selectedCollections.Contains(s.Collection.Name)
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

        var appointBtn = new Button
        {
            Content = "Записаться",
            Margin = new Avalonia.Thickness(0, 10, 0, 0),
            Padding = new Avalonia.Thickness(15, 5),
            Background = Avalonia.Media.Brushes.White,
            BorderBrush = Avalonia.Media.Brushes.Black,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
            Tag = service
        };
        appointBtn.Click += appointBtn_Click;
        infoPanel.Children.Add(appointBtn);

        Grid.SetColumn(infoPanel, 1);
        grid.Children.Add(infoPanel);

        border.Child = grid;
        return border;
    }

    private string GetServiceImageUri(Service service)
    {
        string category = service.Category?.Name ?? "";
        int imageNumber;

        if (service.Id > 10)
        {
            imageNumber = service.Id - 10;
        }
        else
            imageNumber = service.Id;

        if (category == "Кастомизация")
        {
            return $"avares://AtelierSystem/Assets/Data/Custom/Pr{imageNumber}.jpg";
        }
        else if (category == "Создание косплея")
        {
            return $"avares://AtelierSystem/Assets/Data/Cosplay/KL{imageNumber}.jpg";
        }

        return string.Empty;
    }

    private async void appointBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Service service)
        {
            var parent = this.VisualRoot as Window;
            var appointmentPage = new AppointmentPage(service);

            if (parent is UserWindow UserWindow)
            {
                UserWindow.mainContentControl.Content = appointmentPage;
            }
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

    private void searchBtn_Click(object? sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private void filter_Changed(object? sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private void resetFilterBtn_Click(object? sender, RoutedEventArgs e)
    {
        animeFilter.IsChecked = false;
        newYearFilter.IsChecked = false;
        halloweenFilter.IsChecked = false;
        cyberpunkFilter.IsChecked = false;
        noirFilter.IsChecked = false;
        searchBox.Text = string.Empty;

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