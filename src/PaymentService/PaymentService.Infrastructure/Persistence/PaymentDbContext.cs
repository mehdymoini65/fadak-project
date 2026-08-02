using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Persistence;

public sealed class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TerminalNo).IsRequired().HasMaxLength(64);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.RedirectUrl).IsRequired().HasMaxLength(500);
            entity.Property(t => t.ReservationNumber).IsRequired().HasMaxLength(128);
            entity.Property(t => t.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(t => t.Token).IsRequired().HasMaxLength(64);
            entity.Property(t => t.Rrn).HasMaxLength(32);
            entity.Property(t => t.AppCode).HasMaxLength(128);
            entity.HasIndex(t => t.Token).IsUnique();
            entity.HasIndex(t => new { t.Status, t.CreatedAt });
        });
    }
}
