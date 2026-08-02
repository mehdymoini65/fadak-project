using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;
namespace NotificationService.Infrastructure.Persistence;
public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<NotificationLog>(); e.ToTable("NotificationLogs"); e.HasKey(x => x.Id);
        e.Property(x => x.Token).IsRequired().HasMaxLength(64); e.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        e.Property(x => x.Status).IsRequired().HasMaxLength(32); e.Property(x => x.CallbackUrl).HasMaxLength(500); e.Property(x => x.ErrorMessage).HasMaxLength(2000);
        e.HasIndex(x => x.Token); e.HasIndex(x => x.CreatedAt);
    }
}
