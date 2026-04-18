using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AtelierSystem;

public partial class UsersPage : UserControl
{
    private List<User> allUsers;
    private List<User> filteredUsers;

    public UsersPage()
    {
        InitializeComponent();
        LoadUsers();
        roleFilterBox.SelectedIndex = 0;
    }

    private void LoadUsers()
    {
        allUsers = App.dataBaseContext.Users.ToList();

        foreach (var user in allUsers)
        {
            user.Role = App.dataBaseContext.Roles.FirstOrDefault(r => r.Id == user.RoleId);
        }

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredUsers = allUsers.ToList();

        if (roleFilterBox.SelectedIndex > 0)
        {
            string roleName = ((ComboBoxItem)roleFilterBox.SelectedItem).Content.ToString();
            filteredUsers = filteredUsers.Where(u => u.Role?.Name == roleName).ToList();
        }

        string searchText = searchBox.Text?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(searchText))
        {
            filteredUsers = filteredUsers.Where(u =>
                u.Login.Contains(searchText, System.StringComparison.OrdinalIgnoreCase) ||
                u.FullName.Contains(searchText, System.StringComparison.OrdinalIgnoreCase) ||
                (u.Email != null && u.Email.Contains(searchText, System.StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        DisplayUsers();
    }

    private void DisplayUsers()
    {
        usersPanel.Children.Clear();

        if (!filteredUsers.Any())
        {
            var emptyText = new TextBlock
            {
                Text = "Пользователи не найдены.",
                FontSize = 14,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };
            usersPanel.Children.Add(emptyText);
            return;
        }

        foreach (var user in filteredUsers)
        {
            var card = CreateUserCard(user);
            usersPanel.Children.Add(card);
        }
    }

    private Border CreateUserCard(User user)
    {
        var border = new Border
        {
            BorderBrush = Avalonia.Media.Brushes.LightGray,
            BorderThickness = new Avalonia.Thickness(1),
            Padding = new Avalonia.Thickness(15),
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

        var namePanel = new StackPanel { Spacing = 3 };
        namePanel.Children.Add(new TextBlock
        {
            Text = user.FullName,
            FontWeight = Avalonia.Media.FontWeight.Bold
        });
        namePanel.Children.Add(new TextBlock
        {
            Text = $"Логин: {user.Login}",
            FontSize = 12,
            Foreground = Avalonia.Media.Brushes.Gray
        });
        Grid.SetColumn(namePanel, 0);
        grid.Children.Add(namePanel);

        var emailText = new TextBlock
        {
            Text = user.Email ?? "Нет email",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(emailText, 1);
        grid.Children.Add(emailText);

        var balanceText = new TextBlock
        {
            Text = $"{user.Balance:F2} ₽",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(balanceText, 2);
        grid.Children.Add(balanceText);

        // Роль
        var roleText = new TextBlock
        {
            Text = user.Role?.Name ?? "Не указана",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Grid.SetColumn(roleText, 3);
        grid.Children.Add(roleText);

        var actionPanel = new StackPanel { Spacing = 5 };

        if (user.Id != CurrentUser.currentUser.Id)
        {
            var editBtn = new Button
            {
                Content = "Редактировать",
                Padding = new Avalonia.Thickness(10, 5),
                Background = Avalonia.Media.Brushes.White,
                BorderBrush = Avalonia.Media.Brushes.Black,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
                Tag = user
            };
            editBtn.Click += editBtn_Click;
            actionPanel.Children.Add(editBtn);
        }

        Grid.SetColumn(actionPanel, 4);
        grid.Children.Add(actionPanel);

        border.Child = grid;
        return border;
    }

    private void editBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is User user)
        {
            var editPage = new EditUserPage(user);

            if (this.VisualRoot is AdminWindow window)
            {
                window.mainContentControl.Content = editPage;
            }
        }
    }

    private void searchBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void roleFilterBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void refreshBtn_Click(object? sender, RoutedEventArgs e)
    {
        LoadUsers();
    }
}