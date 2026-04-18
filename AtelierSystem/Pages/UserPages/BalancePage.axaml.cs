using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace AtelierSystem;

public partial class BalancePage : UserControl
{
    public event Action BalanceUpdated;

    public BalancePage()
    {
        InitializeComponent();
        LoadCurrentBalance();
    }

    private void LoadCurrentBalance()
    {
        if (CurrentUser.currentUser != null)
        {
            currentBalanceBox.Text = $"{CurrentUser.currentUser.Balance:F2} ₽";
        }
    }

    private async void topUpBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string amountText = amountBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(amountText))
            {
                await ShowMessageBox("Ошибка", "Введите сумму пополнения.", Icon.Warning);
                return;
            }

            if (!decimal.TryParse(amountText, out decimal amount))
            {
                await ShowMessageBox("Ошибка", "Введите корректную сумму.", Icon.Warning);
                return;
            }

            if (amount <= 0)
            {
                await ShowMessageBox("Ошибка", "Сумма должна быть больше нуля.", Icon.Warning);
                return;
            }

            var user = App.dataBaseContext.Users.FirstOrDefault(x => x.Id == CurrentUser.currentUser.Id);
            if (user != null)
            {
                user.Balance += amount;

                var payment = new Payment()
                {
                    UserId = user.Id,
                    Amount = amount,
                    PaymentMethod = "card",
                    TransactionDate = DateTime.Now
                };

                App.dataBaseContext.Payments.Add(payment);
                App.dataBaseContext.SaveChanges();

                CurrentUser.currentUser.Balance = user.Balance;

                await ShowMessageBox("Успешно", $"Баланс пополнен на {amount:F2} ₽.", Icon.Success);

                LoadCurrentBalance();
                amountBox.Text = string.Empty;

                BalanceUpdated?.Invoke();
            }
        }
        catch (Exception ex)
        {
            await ShowMessageBox("Ошибка", $"Произошла ошибка: {ex.Message}", Icon.Error);
        }
    }

    private void cancelBtn_Click(object? sender, RoutedEventArgs e)
    {
        amountBox.Text = string.Empty;
    }

    private async System.Threading.Tasks.Task ShowMessageBox(string title, string message, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
        var parent = this.VisualRoot as Window;
        await box.ShowWindowDialogAsync(parent);
    }
}