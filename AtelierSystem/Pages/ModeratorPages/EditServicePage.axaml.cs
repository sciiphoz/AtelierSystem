using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierSystem;

public partial class EditServicePage : UserControl
{
    private Service service;
    private List<ServiceCategory> categories;
    private List<Collection> collections;

    public EditServicePage(Service service)
    {
        InitializeComponent();
        this.service = service;
        LoadCategories();
        LoadCollections();
        LoadServiceData();
    }

    private void LoadCategories()
    {
        categories = App.dataBaseContext.ServiceCategories.ToList();

        foreach (var category in categories)
        {
            categoryComboBox.Items.Add(category.Name);
        }
    }

    private void LoadCollections()
    {
        collections = App.dataBaseContext.Collections.ToList();

        foreach (var collection in collections)
        {
            collectionComboBox.Items.Add(collection.Name);
        }
    }

    private void LoadServiceData()
    {
        nameBox.Text = service.Name;
        descriptionBox.Text = service.Description ?? "";
        priceBox.Text = service.Price.ToString("F2");
        updatedAtBox.Text = service.UpdatedAt?.ToString("dd.MM.yyyy HH:mm:ss") ?? "Не изменялось";

        var category = categories.FirstOrDefault(c => c.Id == service.CategoryId);
        if (category != null)
        {
            categoryComboBox.SelectedIndex = categories.IndexOf(category);
        }

        var collection = collections.FirstOrDefault(c => c.Id == service.CollectionId);
        if (collection != null)
        {
            collectionComboBox.SelectedIndex = collections.IndexOf(collection);
        }
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

            var dbService = App.dataBaseContext.Services.FirstOrDefault(s => s.Id == service.Id);
            if (dbService != null)
            {
                dbService.Name = name;
                dbService.Description = description;
                dbService.Price = price;
                dbService.CategoryId = categories[categoryComboBox.SelectedIndex].Id;
                dbService.CollectionId = collections[collectionComboBox.SelectedIndex].Id;
                dbService.UpdatedAt = DateTime.Now;

                App.dataBaseContext.SaveChanges();

                await ShowMessageBox("Успешно", "Услуга обновлена.", Icon.Success);

                if (this.VisualRoot is ModeratorWindow window)
                {
                    window.mainContentControl.Content = new ModeratorServicesPage();
                }
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