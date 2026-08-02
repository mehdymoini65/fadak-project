using GatewayService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace GatewayService.Infrastructure.Persistence;
public sealed class GatewayDbContext(DbContextOptions<GatewayDbContext> options) : DbContext(options)
{
    public DbSet<PaymentLog> PaymentLogs => Set<PaymentLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PaymentLog>();
        entity.ToTable("PaymentLogs");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Token).IsRequired().HasMaxLength(64);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Rrn).HasMaxLength(32);
        entity.HasIndex(x => x.Token);
        entity.HasIndex(x => x.ProcessedAt);
    }
}
