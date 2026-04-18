using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AtelierSystem.DBContext;
using AtelierSystem;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;

namespace AtelierSystem;

public partial class AdminWindow : Window
{
    public AdminWindow()
    {
        InitializeComponent();
        LoadLogo();
        LoadAdminInfo();
        mainContentControl.Content = new UsersPage();
    }

    private void LoadLogo()
    {
        try
        {
            var uri = new Uri("avares://AtelierSystem/Assets/Icon/Logo.png");
            logoImage.Source = new Bitmap(AssetLoader.Open(uri));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки логотипа: {ex.Message}");
        }
    }

    private void LoadAdminInfo()
    {
        if (CurrentUser.currentUser != null)
        {
            adminNameText.Text = CurrentUser.currentUser.FullName;
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

    private void usersBtn_Click(object? sender, RoutedEventArgs e)
    {
        mainContentControl.Content = new UsersPage();
    }

    private void addEmployeeBtn_Click(object? sender, RoutedEventArgs e)
    {
        mainContentControl.Content = new AddEmployeePage();
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