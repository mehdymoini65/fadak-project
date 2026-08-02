namespace PaymentService.Application.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>SQLite connection string used by EF Core.</summary>
    public string ConnectionString { get; set; } = "Data Source=payments.db";
}
