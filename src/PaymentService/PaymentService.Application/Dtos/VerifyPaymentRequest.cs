using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.Dtos;

public sealed class VerifyPaymentRequest
{
    [Required(ErrorMessage = "توکن الزامی است")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد اپلیکیشن الزامی است")]
    [StringLength(128, ErrorMessage = "کد اپلیکیشن حداکثر ۱۲۸ کاراکتر است")]
    public string AppCode { get; set; } = string.Empty;
}
