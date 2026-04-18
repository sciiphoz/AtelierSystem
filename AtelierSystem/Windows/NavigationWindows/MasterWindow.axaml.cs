using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using AtelierSystem.DBContext;
using AtelierSystem;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace AtelierSystem;

public partial class MasterWindow : Window
{
    public MasterWindow()
    {
        InitializeComponent();
        LoadLogo();
        LoadMasterInfo();
        mainContentControl.Content = new MasterAppointmentsPage();
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
        }
    }

    private void LoadMasterInfo()
    {
        if (CurrentUser.currentUser != null)
        {
            var master = App.dataBaseContext.Masters
                .FirstOrDefault(m => m.UserId == CurrentUser.currentUser.Id);

            if (master != null)
            {
                masterNameText.Text = CurrentUser.currentUser.FullName;
                masterLevelText.Text = master.QualificationLevel;
            }
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

    private void appointmentsBtn_Click(object? sender, RoutedEventArgs e)
    {
        mainContentControl.Content = new MasterAppointmentsPage();
    }

    private void qualificationBtn_Click(object? sender, RoutedEventArgs e)
    {
        mainContentControl.Content = new QualificationPage();
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