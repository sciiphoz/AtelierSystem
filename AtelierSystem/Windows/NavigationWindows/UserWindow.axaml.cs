using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using AtelierSystem.DBContext;
using AtelierSystem;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using Avalonia.Platform;

namespace AtelierSystem
{
    public partial class UserWindow : Window
    {
        public UserWindow()
        {
            InitializeComponent();
            LoadLogo();
            LoadUserInfo();
            mainContentControl.Content = new ServicesPage();
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
                logoImage.Source = null;
            }
        }

        private void LoadUserInfo()
        {
            if (CurrentUser.currentUser != null)
            {
                userNameText.Text = CurrentUser.currentUser.FullName;
                userBalanceText.Text = $"Баланс: {(CurrentUser.currentUser.Balance ?? 0):F2} ₽";
            }
        }

        public void UpdateUserInfo()
        {
            LoadUserInfo();
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
            mainContentControl.Content = new ServicesPage();
        }

        private void myAppointmentsBtn_Click(object? sender, RoutedEventArgs e)
        {
            mainContentControl.Content = new MyAppointmentsPage();
        }

        private void profileBtn_Click(object? sender, RoutedEventArgs e)
        {
            mainContentControl.Content = new ProfilePage();
        }

        private void balanceBtn_Click(object? sender, RoutedEventArgs e)
        {
            var balancePage = new BalancePage();
            balancePage.BalanceUpdated += OnBalanceUpdated;
            mainContentControl.Content = balancePage;
        }

        private void OnBalanceUpdated()
        {
            LoadUserInfo();
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
}