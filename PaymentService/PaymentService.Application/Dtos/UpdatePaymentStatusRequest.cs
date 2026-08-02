using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.Dtos;

public sealed class UpdatePaymentStatusRequest
{
    [Required(ErrorMessage = "توکن الزامی است")]
    public string Token { get; set; } = string.Empty;

    /// <summary>True -> Success, False -> Failed.</summary>
    public bool IsSuccess { get; set; }

    [StringLength(12, MinimumLength = 12, ErrorMessage = "شماره پیگیری (RRN) باید ۱۲ رقم باشد")]
    [RegularExpression("^[0-9]+$", ErrorMessage = "شماره پیگیری (RRN) فقط می‌تواند عدد باشد")]
    public string? Rrn { get; set; }
}
