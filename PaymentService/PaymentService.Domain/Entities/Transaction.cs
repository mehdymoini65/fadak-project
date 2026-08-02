using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }
    public string TerminalNo { get; private set; }
    public decimal Amount { get; private set; }
    public string RedirectUrl { get; private set; }
    public string ReservationNumber { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Token { get; private set; }
    public string? Rrn { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? AppCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Transaction(
        string terminalNo,
        decimal amount,
        string redirectUrl,
        string reservationNumber,
        string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(terminalNo))
        {
            throw new ArgumentException("TerminalNo cannot be empty.", nameof(terminalNo));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(redirectUrl))
        {
            throw new ArgumentException("RedirectUrl cannot be empty.", nameof(redirectUrl));
        }

        if (string.IsNullOrWhiteSpace(reservationNumber))
        {
            throw new ArgumentException("ReservationNumber cannot be empty.", nameof(reservationNumber));
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("PhoneNumber cannot be empty.", nameof(phoneNumber));
        }

        Id = Guid.NewGuid();
        TerminalNo = terminalNo;
        Amount = amount;
        RedirectUrl = redirectUrl;
        ReservationNumber = reservationNumber;
        PhoneNumber = phoneNumber;
        Token = Guid.NewGuid().ToString("N");
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    private Transaction()
    {
        // EF Core materialization
        TerminalNo = string.Empty;
        RedirectUrl = string.Empty;
        ReservationNumber = string.Empty;
        PhoneNumber = string.Empty;
        Token = string.Empty;
    }
}
