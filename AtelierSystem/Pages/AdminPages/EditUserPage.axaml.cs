using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierSystem;

public partial class EditUserPage : UserControl
{
    private User user;
    private List<Role> roles;

    public EditUserPage(User user)
    {
        InitializeComponent();
        this.user = user;
        LoadRoles();
        LoadUserData();
    }

    private void LoadRoles()
    {
        roles = App.dataBaseContext.Roles.ToList();

        foreach (var role in roles)
        {
            roleComboBox.Items.Add(role.Name);
        }
    }

    private void LoadUserData()
    {
        fullNameBox.Text = user.FullName;
        loginBox.Text = user.Login;
        emailBox.Text = user.Email ?? "";
        balanceBox.Text = user.Balance?.ToString("F2") ?? "0.00";

        var role = roles.FirstOrDefault(r => r.Id == user.RoleId);
        if (role != null)
        {
            roleComboBox.SelectedIndex = roles.IndexOf(role);
        }
    }

    private async void saveBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string fullName = fullNameBox.Text?.Trim() ?? string.Empty;
            string login = loginBox.Text?.Trim() ?? string.Empty;
            string email = emailBox.Text?.Trim() ?? string.Empty;
            string balanceText = balanceBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(fullName))
            {
                await ShowMessageBox("Ошибка", "Введите ФИО.", Icon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(login))
            {
                await ShowMessageBox("Ошибка", "Введите логин.", Icon.Warning);
                return;
            }

            if (!decimal.TryParse(balanceText, out decimal balance) || balance < 0)
            {
                await ShowMessageBox("Ошибка", "Введите корректный баланс.", Icon.Warning);
                return;
            }

            if (roleComboBox.SelectedIndex == -1)
            {
                await ShowMessageBox("Ошибка", "Выберите роль.", Icon.Warning);
                return;
            }

            var existingUser = App.dataBaseContext.Users
                .FirstOrDefault(u => u.Login == login && u.Id != user.Id);

            if (existingUser != null)
            {
                await ShowMessageBox("Ошибка", "Пользователь с таким логином уже существует.", Icon.Warning);
                return;
            }

            var dbUser = App.dataBaseContext.Users.FirstOrDefault(u => u.Id == user.Id);
            if (dbUser != null)
            {
                dbUser.FullName = fullName;
                dbUser.Login = login;
                dbUser.Email = email;
                dbUser.Balance = balance;
                dbUser.RoleId = roles[roleComboBox.SelectedIndex].Id;

                App.dataBaseContext.SaveChanges();

                await ShowMessageBox("Успешно", "Данные пользователя обновлены.", Icon.Success);

                if (this.VisualRoot is AdminWindow window)
                {
                    window.mainContentControl.Content = new UsersPage();
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
        if (this.VisualRoot is AdminWindow window)
        {
            window.mainContentControl.Content = new UsersPage();
        }
    }

    private async System.Threading.Tasks.Task ShowMessageBox(string title, string message, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
        var parent = this.VisualRoot as Window;
        await box.ShowWindowDialogAsync(parent);
    }
}