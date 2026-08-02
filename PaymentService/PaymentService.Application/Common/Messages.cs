namespace PaymentService.Application.Common;

public static class Messages
{
    public const string TokenCreated = "توکن با موفقیت ایجاد شد";
    public const string InvalidToken = "توکن نامعتبر است";
    public const string PaymentExpired = "زمان پرداخت منقضی شده است";
    public const string StatusUpdated = "وضعیت با موفقیت به‌روزرسانی شد";
    public const string StatusAlreadyUpdated = "وضعیت قبلاً به‌روزرسانی شده است";
    public const string StatusUnchangeable = "وضعیت فعلی تراکنش اجازه تغییر را نمی‌دهد";
    public const string InvalidStatus = "وضعیت ارسالی نامعتبر است";
    public const string RrnRequired = "شماره پیگیری (RRN) الزامی است";
    public const string InvalidRrn = "شماره پیگیری (RRN) باید ۱۲ رقم باشد";
    public const string AmountInvalid = "مبلغ باید بزرگ‌تر از صفر باشد";
    public const string InternalError = "خطای داخلی رخ داد";
    public const string ValidationFailed = "اطلاعات ارسال‌شده معتبر نیست";
}
