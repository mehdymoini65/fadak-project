using NotificationService.Domain.Entities;
using Xunit;

namespace NotificationService.Domain.UnitTests;

public sealed class NotificationLogTests
{
    [Fact]
    public void NewLog_HasIdentifierAndCreatedTime()
    {
        var log = new NotificationLog { Token = Guid.NewGuid().ToString("N"), EventType = "PaymentProcessed", Status = "Success" };
        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.NotEqual(default, log.CreatedAt);
    }
}
