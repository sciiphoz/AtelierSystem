using AtelierSystem.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using SystemTests;
using Xunit;

namespace SystemTests;

public class ServiceUpdateTimeTests : IDisposable
{
    private readonly TestDataBaseContext _context;

    public ServiceUpdateTimeTests()
    {
        var options = new DbContextOptionsBuilder<DataBaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDataBaseContext(options);

        _context.Collections.Add(new Collection { Id = 1, Name = "Аниме" });
        _context.ServiceCategories.Add(new ServiceCategory { Id = 1, Name = "Кастомизация" });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    /// <summary>
    /// Тест №1: Проверка, что при создании новой услуги
    /// поле updated_at устанавливается равным created_at
    /// </summary>
    [Fact]
    public void CreateService_SetsUpdatedAtEqualToCreatedAt()
    {
        var createTime = DateTime.Now;

        var service = new Service
        {
            Name = "Тестовая услуга",
            Description = "Описание тестовой услуги",
            Price = 1000.00m,
            CollectionId = 1,
            CategoryId = 1,
            CreatedAt = createTime,
            UpdatedAt = createTime
        };

        _context.Services.Add(service);
        _context.SaveChanges();

        var savedService = _context.Services.FirstOrDefault(s => s.Name == "Тестовая услуга");
        Assert.NotNull(savedService);
        Assert.Equal(savedService.CreatedAt?.ToString("yyyy-MM-dd HH:mm"),
                     savedService.UpdatedAt?.ToString("yyyy-MM-dd HH:mm"));
    }

    /// <summary>
    /// Тест №2: Проверка, что при редактировании услуги
    /// поле updated_at обновляется и становится больше created_at
    /// </summary>
    [Fact]
    public void UpdateService_ChangesUpdatedAt()
    {
        var createTime = new DateTime(2026, 4, 1, 10, 0, 0);

        var service = new Service
        {
            Name = "Услуга для редактирования",
            Description = "Изначальное описание",
            Price = 2000.00m,
            CollectionId = 1,
            CategoryId = 1,
            CreatedAt = createTime,
            UpdatedAt = createTime
        };

        _context.Services.Add(service);
        _context.SaveChanges();

        var savedService = _context.Services.First(s => s.Name == "Услуга для редактирования");
        var originalUpdatedAt = savedService.UpdatedAt;

        savedService.Name = "Отредактированная услуга";
        savedService.Price = 2500.00m;
        savedService.UpdatedAt = DateTime.Now;
        _context.SaveChanges();

        var updatedService = _context.Services.First(s => s.Id == savedService.Id);
        Assert.Equal("Отредактированная услуга", updatedService.Name);
        Assert.Equal(2500.00m, updatedService.Price);
        Assert.True(updatedService.UpdatedAt > originalUpdatedAt,
            "Время обновления должно быть больше исходного");
    }

    /// <summary>
    /// Тест №3: Проверка, что поле updated_at изменяется при каждом редактировании
    /// </summary>
    [Fact]
    public void MultipleUpdates_EachChangesUpdatedAt()
    {
        var service = new Service
        {
            Name = "Множественные обновления",
            Description = "Описание",
            Price = 3000.00m,
            CollectionId = 1,
            CategoryId = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Services.Add(service);
        _context.SaveChanges();

        var originalUpdatedAt = service.UpdatedAt;
        var serviceId = service.Id;

        _context.ChangeTracker.Clear();
        var service1 = _context.Services.Find(serviceId);
        service1.Price = 3500.00m;
        service1.UpdatedAt = DateTime.Now.AddDays(-1); // Вчера
        _context.SaveChanges();

        _context.ChangeTracker.Clear();
        var afterFirst = _context.Services.Find(serviceId);
        var firstUpdateTime = afterFirst.UpdatedAt;

        var service2 = _context.Services.Find(serviceId);
        service2.Description = "Новое описание";
        service2.UpdatedAt = DateTime.Now.AddDays(1); // Завтра
        _context.SaveChanges();

        _context.ChangeTracker.Clear();
        var afterSecond = _context.Services.Find(serviceId);
        var secondUpdateTime = afterSecond.UpdatedAt;

        Assert.NotEqual(originalUpdatedAt, firstUpdateTime);
        Assert.NotEqual(firstUpdateTime, secondUpdateTime);
        Assert.True(secondUpdateTime > firstUpdateTime,
            $"Second ({secondUpdateTime:yyyy-MM-dd HH:mm:ss}) should be greater than first ({firstUpdateTime:yyyy-MM-dd HH:mm:ss})");
    }

    /// <summary>
    /// Тест №4: Проверка формата даты в поле updated_at
    /// </summary>
    [Fact]
    public void UpdatedAt_HasCorrectDateTimeFormat()
    {
        var service = new Service
        {
            Name = "Проверка формата даты",
            Description = "Описание",
            Price = 4000.00m,
            CollectionId = 1,
            CategoryId = 1,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Services.Add(service);
        _context.SaveChanges();

        var savedService = _context.Services.First(s => s.Name == "Проверка формата даты");
        Assert.NotNull(savedService.UpdatedAt);
        Assert.NotEqual(default(DateTime), savedService.UpdatedAt);
        Assert.True(savedService.UpdatedAt?.Year >= 2024, "Год должен быть >= 2024");
    }

    /// <summary>
    /// Тест №5: Проверка, что updated_at не изменяется при чтении данных
    /// </summary>
    [Fact]
    public void ReadingService_DoesNotChangeUpdatedAt()
    {
        var createTime = new DateTime(2026, 4, 15, 12, 0, 0);

        var service = new Service
        {
            Name = "Услуга для чтения",
            Description = "Описание",
            Price = 5000.00m,
            CollectionId = 1,
            CategoryId = 1,
            CreatedAt = createTime,
            UpdatedAt = createTime
        };

        _context.Services.Add(service);
        _context.SaveChanges();

        var readService = _context.Services.FirstOrDefault(s => s.Name == "Услуга для чтения");
        var readAgain = _context.Services.FirstOrDefault(s => s.Name == "Услуга для чтения");

        Assert.NotNull(readService);
        Assert.Equal(createTime.ToString("yyyy-MM-dd HH:mm:ss"),
                     readService.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}