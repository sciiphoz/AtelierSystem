using AtelierSystem.DBContext;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;

namespace AtelierSystem;

public partial class ModeratorWindow : Window
{
    public ModeratorWindow()
    {
        InitializeComponent();
        LoadLogo();
        LoadModeratorInfo();
        mainContentControl.Content = new ModeratorServicesPage();
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

    private void LoadModeratorInfo()
    {
        if (CurrentUser.currentUser != null)
        {
            moderatorNameText.Text = CurrentUser.currentUser.FullName;
        }
    }

    private void minimizeBtn_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private async void closeBtn_Click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Подтверждение выхода",
            "Вы действительно хотите выйти из приложения?",
            ButtonEnum.YesNo,
            MsBox.Avalonia.Enums.Icon.Question);

        var result = await box.ShowAsync();

        if (result == ButtonResult.Yes)
        {
            Close();
        }
    }

    private void servicesBtn_Click(object? sender, RoutedEventArgs e)
    {
        mainContentControl.Content = new ModeratorServicesPage();
    }

    private void addServiceBtn_Click(object? sender, RoutedEventArgs e)
    {
        mainContentControl.Content = new AddServicePage();
    }

    private async void logoutBtn_Click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Подтверждение",
            "Вы действительно хотите выйти из аккаунта?",
            ButtonEnum.YesNo,
            MsBox.Avalonia.Enums.Icon.Question);

        var result = await box.ShowAsync();

        if (result == ButtonResult.Yes)
        {
            CurrentUser.currentUser = null;

            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}