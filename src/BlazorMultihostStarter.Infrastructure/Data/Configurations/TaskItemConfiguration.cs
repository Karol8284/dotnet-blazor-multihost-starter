using BlazorMultihostStarter.Domain.Modules.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorMultihostStarter.Infrastructure.Data.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(task => task.Id);
        builder.Property(task => task.OwnerId)
            .HasMaxLength(80)
            .IsRequired();
        builder.Property(task => task.Title)
            .HasMaxLength(120)
            .IsRequired();
        builder.Property(task => task.Version)
            .IsConcurrencyToken()
            .HasDefaultValue(1);
        builder.Property(task => task.CreatedAtUtc).IsRequired();
        builder.Property(task => task.IsCompleted).IsRequired();
        builder.Property(task => task.CompletedAtUtc);

        builder.HasIndex(task => new { task.OwnerId, task.CreatedAtUtc });
    }
}
