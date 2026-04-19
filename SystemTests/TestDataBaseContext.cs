using AtelierSystem.DBContext;
using Microsoft.EntityFrameworkCore;

namespace SystemTests;

public class TestDataBaseContext : DataBaseContext
{
    public TestDataBaseContext(DbContextOptions<DataBaseContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Ничего не делаем — используем только переданные options
        // Это предотвращает вызов UseNpgsql из основного контекста
    }
}