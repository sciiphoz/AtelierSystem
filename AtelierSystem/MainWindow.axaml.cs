using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AtelierSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainContentControl.Content = new AuthPage();
        }

        private void signInBtn_Click(object? sender, RoutedEventArgs e)
        {
            mainContentControl.Content = new AuthPage();
        }

        private void signUpBtn_Click(object? sender, RoutedEventArgs e)
        {
            mainContentControl.Content = new RegPage();
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

            var parent = this.VisualRoot as Window;
            var result = await box.ShowWindowDialogAsync(parent);

            if (result == ButtonResult.Yes)
            {
                Close();
            }
        }
    }
}