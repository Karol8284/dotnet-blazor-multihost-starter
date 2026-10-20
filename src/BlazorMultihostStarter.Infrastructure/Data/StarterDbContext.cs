using BlazorMultihostStarter.Domain.Modules.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlazorMultihostStarter.Infrastructure.Data;

public sealed class StarterDbContext(DbContextOptions<StarterDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StarterDbContext).Assembly);

        if (Database.IsSqlite())
        {
            modelBuilder.Entity<TaskItem>()
                .Property(task => task.CreatedAtUtc)
                .HasConversion(new DateTimeOffsetToBinaryConverter());
            modelBuilder.Entity<TaskItem>()
                .Property(task => task.CompletedAtUtc)
                .HasConversion(new DateTimeOffsetToBinaryConverter());
        }
    }
}
