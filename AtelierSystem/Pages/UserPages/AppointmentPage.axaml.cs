using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AtelierSystem;

public partial class AppointmentPage : UserControl
{
    private Service selectedService;
    private List<Master> availableMasters;

    public AppointmentPage(Service service)
    {
        InitializeComponent();
        selectedService = service;
        LoadServiceInfo();
        LoadMasters();
    }

    private void LoadServiceInfo()
    {
        serviceNameBox.Text = selectedService.Name;
        servicePriceBox.Text = $"{selectedService.Price:F2} ₽";
    }

    private void LoadMasters()
    {
        var masterServiceIds = App.dataBaseContext.MasterServices
            .Where(ms => ms.ServiceId == selectedService.Id)
            .Select(ms => ms.MasterId)
            .ToList();

        availableMasters = App.dataBaseContext.Masters
            .Where(m => masterServiceIds.Contains(m.Id))
            .ToList();

        foreach (var master in availableMasters)
        {
            var user = App.dataBaseContext.Users.FirstOrDefault(u => u.Id == master.UserId);
            masterComboBox.Items.Add($"{user?.FullName} ({master.QualificationLevel})");
        }

        if (masterComboBox.Items.Count > 0)
        {
            masterComboBox.SelectedIndex = 0;
        }
    }

    private async void confirmBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (masterComboBox.SelectedIndex == -1)
            {
                await ShowMessageBox("Ошибка", "Выберите мастера.", Icon.Warning);
                return;
            }

            string dateTimeText = appointmentDateTimeBox.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(dateTimeText))
            {
                await ShowMessageBox("Ошибка", "Введите дату и время.", Icon.Warning);
                return;
            }

            if (!DateTime.TryParseExact(dateTimeText, "dd.MM.yyyy HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime appointmentTime))
            {
                await ShowMessageBox("Ошибка",
                    "Неверный формат даты. Используйте ДД.ММ.ГГГГ ЧЧ:ММ", Icon.Warning);
                return;
            }

            if (appointmentTime < DateTime.Now)
            {
                await ShowMessageBox("Ошибка",
                    "Дата и время не могут быть в прошлом.", Icon.Warning);
                return;
            }

            if ((CurrentUser.currentUser.Balance ?? 0) < selectedService.Price)
            {
                await ShowMessageBox("Ошибка",
                    "Недостаточно средств на балансе. Пополните баланс.", Icon.Warning);
                return;
            }

            var selectedMaster = availableMasters[masterComboBox.SelectedIndex];

            var existingAppointments = App.dataBaseContext.Appointments
                .Where(a => a.MasterId == selectedMaster.Id && a.Status != "cancelled")
                .OrderBy(a => a.QueueNumber)
                .ToList();

            int queueNumber = 1;
            if (existingAppointments.Any())
            {
                queueNumber = existingAppointments.Max(a => a.QueueNumber ?? 0) + 1;
            }

            var appointment = new Appointment()
            {
                UserId = CurrentUser.currentUser.Id,
                MasterId = selectedMaster.Id,
                ServiceId = selectedService.Id,
                AppointmentTime = appointmentTime,
                QueueNumber = queueNumber,
                Status = "pending",
                CreatedAt = DateTime.Now
            };

            App.dataBaseContext.Appointments.Add(appointment);

            var user = App.dataBaseContext.Users.FirstOrDefault(u => u.Id == CurrentUser.currentUser.Id);
            if (user != null)
            {
                user.Balance -= selectedService.Price;
                CurrentUser.currentUser.Balance = user.Balance;
            }

            App.dataBaseContext.SaveChanges();

            if (this.VisualRoot is UserWindow dataWindow)
            {
                dataWindow.UpdateUserInfo();
            }

            await ShowMessageBox("Успешно",
                $"Вы записаны на услугу.\nНомер в очереди: {queueNumber}", Icon.Success);

            if (this.VisualRoot is UserWindow dw)
            {
                dw.mainContentControl.Content = new ServicesPage();
            }
        }
        catch (Exception ex)
        {
            await ShowMessageBox("Ошибка", $"Произошла ошибка: {ex.Message}", Icon.Error);
        }
    }

    private void cancelBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (this.VisualRoot is UserWindow dataWindow)
        {
            dataWindow.mainContentControl.Content = new ServicesPage();
        }
    }

    private async System.Threading.Tasks.Task ShowMessageBox(string title, string message, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok, icon);
        var parent = this.VisualRoot as Window;
        await box.ShowWindowDialogAsync(parent);
    }
}