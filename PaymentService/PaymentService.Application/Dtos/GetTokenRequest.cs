using System.ComponentModel.DataAnnotations;

namespace PaymentService.Application.Dtos;

public sealed class GetTokenRequest
{
    [Required(ErrorMessage = "کد ترمینال الزامی است")]
    [StringLength(64, ErrorMessage = "کد ترمینال حداکثر ۶۴ کاراکتر است")]
    public string TerminalNo { get; set; } = string.Empty;

    [Range(1, double.MaxValue, ErrorMessage = "مبلغ باید بزرگ‌تر از صفر باشد")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "آدرس بازگشت الزامی است")]
    [Url(ErrorMessage = "آدرس بازگشت معتبر نیست")]
    public string RedirectUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره رزرو الزامی است")]
    [StringLength(128, ErrorMessage = "شماره رزرو حداکثر ۱۲۸ کاراکتر است")]
    public string ReservationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
    public string PhoneNumber { get; set; } = string.Empty;
}
