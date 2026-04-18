using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtelierSystem;

public partial class AddEmployeePage : UserControl
{
    private List<Role> roles;

    public AddEmployeePage()
    {
        InitializeComponent();
        LoadRoles();
        qualificationBox.SelectedIndex = 0;
    }

    private void LoadRoles()
    {
        roles = App.dataBaseContext.Roles
            .Where(r => r.Name == "moderator" || r.Name == "master")
            .ToList();

        foreach (var role in roles)
        {
            roleComboBox.Items.Add(GetRoleDisplayName(role.Name));
        }

        if (roleComboBox.Items.Count > 0)
        {
            roleComboBox.SelectedIndex = 0;
        }

        roleComboBox.SelectionChanged += roleComboBox_SelectionChanged;
    }

    private string GetRoleDisplayName(string roleName)
    {
        return roleName switch
        {
            "moderator" => "Модератор",
            "master" => "Мастер",
            _ => roleName
        };
    }

    private void roleComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (roleComboBox.SelectedIndex >= 0)
        {
            var selectedRole = roles[roleComboBox.SelectedIndex];
            qualificationBox.IsEnabled = selectedRole.Name == "master";
        }
    }

    private async void saveBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string fullName = fullNameBox.Text?.Trim() ?? string.Empty;
            string login = loginBox.Text?.Trim() ?? string.Empty;
            string password = passwordBox.Text ?? string.Empty;
            string confirmPassword = confirmPasswordBox.Text ?? string.Empty;
            string email = emailBox.Text?.Trim() ?? string.Empty;

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

            if (string.IsNullOrEmpty(password))
            {
                await ShowMessageBox("Ошибка", "Введите пароль.", Icon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                await ShowMessageBox("Ошибка", "Пароли не совпадают.", Icon.Warning);
                return;
            }

            if (roleComboBox.SelectedIndex == -1)
            {
                await ShowMessageBox("Ошибка", "Выберите роль.", Icon.Warning);
                return;
            }

            var existingUser = App.dataBaseContext.Users
                .FirstOrDefault(u => u.Login == login);

            if (existingUser != null)
            {
                await ShowMessageBox("Ошибка", "Пользователь с таким логином уже существует.", Icon.Warning);
                return;
            }

            var selectedRole = roles[roleComboBox.SelectedIndex];

            var newUser = new User()
            {
                FullName = fullName,
                Login = login,
                PasswordHash = password,
                Email = email,
                RoleId = selectedRole.Id,
                Balance = 0,
                CreatedAt = DateTime.Now
            };

            App.dataBaseContext.Users.Add(newUser);
            App.dataBaseContext.SaveChanges();

            if (selectedRole.Name == "master")
            {
                string qualificationLevel = ((ComboBoxItem)qualificationBox.SelectedItem).Content.ToString();

                var master = new Master()
                {
                    UserId = newUser.Id,
                    QualificationLevel = qualificationLevel,
                    Description = ""
                };

                App.dataBaseContext.Masters.Add(master);
                App.dataBaseContext.SaveChanges();
            }

            await ShowMessageBox("Успешно", "Сотрудник успешно добавлен.", Icon.Success);

            if (this.VisualRoot is AdminWindow window)
            {
                window.mainContentControl.Content = new UsersPage();
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