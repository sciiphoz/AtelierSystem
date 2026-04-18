using Avalonia.Controls;
using Avalonia.Interactivity;
using AtelierSystem.DBContext;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;

namespace AtelierSystem;

public partial class ReviewPage : UserControl
{
    private Appointment appointment;
    private Service service;
    private Master master;
    private User masterUser;

    public ReviewPage(Appointment appointment)
    {
        InitializeComponent();
        this.appointment = appointment;
        LoadData();
    }

    private void LoadData()
    {
        service = App.dataBaseContext.Services
            .FirstOrDefault(s => s.Id == appointment.ServiceId);

        master = App.dataBaseContext.Masters
            .FirstOrDefault(m => m.Id == appointment.MasterId);

        masterUser = master != null
            ? App.dataBaseContext.Users.FirstOrDefault(u => u.Id == master.UserId)
            : null;

        serviceNameBox.Text = service?.Name ?? "Не указана";
        masterNameBox.Text = masterUser?.FullName ?? "Не указан";

        ratingComboBox.SelectedIndex = 4;
    }

    private async void submitBtn_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            int rating = ratingComboBox.SelectedIndex + 1;
            string comment = commentBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(comment))
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка",
                    "Напишите комментарий.",
                    ButtonEnum.Ok,
                    Icon.Warning);

                var parent = this.VisualRoot as Window;
                await box.ShowWindowDialogAsync(parent);
                return;
            }

            var existingReview = App.dataBaseContext.Reviews
                .FirstOrDefault(r => r.UserId == CurrentUser.currentUser.Id &&
                                    r.MasterId == appointment.MasterId &&
                                    r.ServiceId == appointment.ServiceId);

            if (existingReview != null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Ошибка",
                    "Вы уже оставили отзыв на эту услугу у этого мастера.",
                    ButtonEnum.Ok,
                    Icon.Warning);

                var parent = this.VisualRoot as Window;
                await box.ShowWindowDialogAsync(parent);
                return;
            }

            var review = new Review()
            {
                UserId = CurrentUser.currentUser.Id,
                MasterId = appointment.MasterId,
                ServiceId = appointment.ServiceId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            App.dataBaseContext.Reviews.Add(review);
            App.dataBaseContext.SaveChanges();

            var successBox = MessageBoxManager.GetMessageBoxStandard(
                "Успешно",
                "Спасибо за ваш отзыв!",
                ButtonEnum.Ok,
                Icon.Success);

            var successParent = this.VisualRoot as Window;
            await successBox.ShowWindowDialogAsync(successParent);

            var window = this.VisualRoot as Window;
            if (window is UserWindow UserWindow)
            {
                UserWindow.mainContentControl.Content = new MyAppointmentsPage();
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Ошибка",
                $"Произошла ошибка: {ex.Message}",
                ButtonEnum.Ok,
                Icon.Error);

            var parent = this.VisualRoot as Window;
            await box.ShowWindowDialogAsync(parent);
        }
    }

    private void cancelBtn_Click(object? sender, RoutedEventArgs e)
    {
        var parent = this.VisualRoot as Window;
        if (parent is UserWindow UserWindow)
        {
            UserWindow.mainContentControl.Content = new MyAppointmentsPage();
        }
    }
}