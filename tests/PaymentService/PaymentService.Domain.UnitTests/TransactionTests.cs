using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using Xunit;

namespace PaymentService.Domain.UnitTests;

public sealed class TransactionTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesPendingTransaction()
    {
        var transaction = new Transaction("1001", 120_000, "https://localhost/callback", "RES-1", "09120000000");

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.False(string.IsNullOrWhiteSpace(transaction.Token));
        Assert.Equal(PaymentStatus.Pending, transaction.Status);
        Assert.Equal(120_000, transaction.Amount);
    }

    [Fact]
    public void Constructor_WithNonPositiveAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Transaction("1001", 0, "https://localhost/callback", "RES-1", "09120000000"));
    }
}
