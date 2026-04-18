using AtelierSystem.DBContext;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace AtelierSystem;

public partial class RegPage : UserControl
{
    public RegPage()
    {
        InitializeComponent();
    }

    private async void sendBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string fullName = fullNameBox.Text?.Trim() ?? string.Empty;
            string login = loginBox.Text?.Trim() ?? string.Empty;
            string password = passwordBox.Text ?? string.Empty;
            string confirmPassword = confirmPasswordBox.Text ?? string.Empty;

            if (string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка регистрации",
                    "Заполните все поля.",
                    ButtonEnum.Ok,
                    Icon.Warning);

                var parent = this.VisualRoot as Window;
                await box.ShowWindowDialogAsync(parent);
                return;
            }

            if (password != confirmPassword)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка регистрации",
                    "Пароли не совпадают.",
                    ButtonEnum.Ok,
                    Icon.Warning);

                var parent = this.VisualRoot as Window;
                await box.ShowWindowDialogAsync(parent);
                return;
            }

            if (App.dataBaseContext.Users.Any(x => x.Login == login))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка регистрации",
                    "Пользователь с таким логином уже существует.",
                    ButtonEnum.Ok,
                    Icon.Warning);

                var parent = this.VisualRoot as Window;
                await box.ShowWindowDialogAsync(parent);
                return;
            }

            var newUser = new User()
            {
                FullName = fullName,
                Login = login,
                PasswordHash = password,
                RoleId = 1,
                Balance = 0,
                CreatedAt = DateTime.Now
            };

            App.dataBaseContext.Users.Add(newUser);
            App.dataBaseContext.SaveChanges();

            CurrentUser.currentUser = App.dataBaseContext.Users.FirstOrDefault(x => x.Login == login && x.PasswordHash == password);

            var parentWindow = this.VisualRoot as Window;
            var nav = new UserWindow();
            nav.Show();
            parentWindow?.Close();
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Критическая ошибка",
                $"Произошла ошибка: {ex.Message}",
                ButtonEnum.Ok,
                Icon.Error);

            var parent = this.VisualRoot as Window;
            await box.ShowWindowDialogAsync(parent);
        }
    }
}