namespace NotificationService.Options;

public sealed class CallbackOptions
{
    public const string SectionName = "Callback";
    public int TimeoutSeconds { get; set; } = 15;
    public int RetryCount { get; set; } = 3;
}
