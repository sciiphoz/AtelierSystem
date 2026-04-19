using AtelierSystem;
using AtelierSystem.DBContext;
using AtelierSystem;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace AtelierSystem;

public partial class AuthPage : UserControl
{
    public AuthPage()
    {
        InitializeComponent();
    }

    private async void sendBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string login = loginBox.Text?.Trim() ?? string.Empty;
            string password = passwordBox.Text ?? string.Empty;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка авторизации",
                    "Заполните все поля.",
                    ButtonEnum.Ok,
                    Icon.Warning);

                var parent = this.VisualRoot as Window;
                await box.ShowWindowDialogAsync(parent);
                return;
            }

            var user = App.dataBaseContext.Users.FirstOrDefault(x =>
                x.Login == login &&
                x.PasswordHash == password);

            if (user != null)
            {
                CurrentUser.currentUser = user;

                var parent = this.VisualRoot as Window;
                Window nav;

                if (user.RoleId == 2) 
                {
                    nav = new ModeratorWindow();
                }
                else 
                {
                    nav = new UserWindow();
                }

                nav.Show();
                parent?.Close();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка авторизации",
                    "Неверный логин или пароль.",
                    ButtonEnum.Ok,
                    Icon.Error);

                var parent = this.VisualRoot as Window;
                await box.ShowWindowDialogAsync(parent);
            }
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