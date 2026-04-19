using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using AtelierSystem;

namespace AtelierSystem;

public partial class AddServicePage : UserControl
{
    private List<ServiceCategory> categories;
    private List<Collection> collections;

    public AddServicePage()
    {
        InitializeComponent();
        LoadCategories();
        LoadCollections();
    }

    private void LoadCategories()
    {
        categories = App.dataBaseContext.ServiceCategories.ToList();

        foreach (var category in categories)
        {
            categoryComboBox.Items.Add(category.Name);
        }

        if (categoryComboBox.Items.Count > 0)
            categoryComboBox.SelectedIndex = 0;
    }

    private void LoadCollections()
    {
        collections = App.dataBaseContext.Collections.ToList();

        foreach (var collection in collections)
        {
            collectionComboBox.Items.Add(collection.Name);
        }

        if (collectionComboBox.Items.Count > 0)
            collectionComboBox.SelectedIndex = 0;
    }

    private async void saveBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string name = nameBox.Text?.Trim() ?? string.Empty;
            string description = descriptionBox.Text?.Trim() ?? string.Empty;
            string priceText = priceBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                await ShowMessageBox("Ошибка", "Введите название услуги.", Icon.Warning);
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
            {
                await ShowMessageBox("Ошибка", "Введите корректную цену.", Icon.Warning);
                return;
            }

            if (categoryComboBox.SelectedIndex == -1)
            {
                await ShowMessageBox("Ошибка", "Выберите категорию.", Icon.Warning);
                return;
            }

            var service = new Service()
            {
                Name = name,
                Description = description,
                Price = price,
                CategoryId = categories[categoryComboBox.SelectedIndex].Id,
                CollectionId = collections[collectionComboBox.SelectedIndex].Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            App.dataBaseContext.Services.Add(service);
            App.dataBaseContext.SaveChanges();

            await ShowMessageBox("Успешно", "Услуга добавлена.", Icon.Success);

            if (this.VisualRoot is ModeratorWindow window)
            {
                window.mainContentControl.Content = new ModeratorServicesPage();
            }
        }
        catch (Exception ex)
        {
            await ShowMessageBox("Ошибка", $"Произошла ошибка: {ex.Message}", Icon.Error);
        }
    }

    private void cancelBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (this.VisualRoot is ModeratorWindow window)
        {
            window.mainContentControl.Content = new ModeratorServicesPage();
        }
    }

    private async System.Threading.Tasks.Task ShowMessageBox(string title, string message, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
        var parent = this.VisualRoot as Window;
        await box.ShowWindowDialogAsync(parent);
    }
}