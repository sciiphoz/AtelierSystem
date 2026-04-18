using Avalonia.Controls;
using AtelierSystem.DBContext;

namespace AtelierSystem;

public partial class ProfilePage : UserControl
{
    public ProfilePage()
    {
        InitializeComponent();
        LoadUserData();
    }

    private void LoadUserData()
    {
        if (CurrentUser.currentUser != null)
        {
            fullNameBox.Text = CurrentUser.currentUser.FullName;
            loginBox.Text = CurrentUser.currentUser.Login;
            balanceBox.Text = $"{CurrentUser.currentUser.Balance:F2} ₽";
        }
    }
}