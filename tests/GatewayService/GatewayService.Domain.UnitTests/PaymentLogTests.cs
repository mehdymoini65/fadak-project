using GatewayService.Domain.Entities;
using Xunit;

namespace GatewayService.Domain.UnitTests;

public sealed class PaymentLogTests
{
    [Fact]
    public void NewLog_HasIdentifierAndProcessedTime()
    {
        var log = new PaymentLog { Token = Guid.NewGuid().ToString("N"), Amount = 10_000, IsSuccess = true };
        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.NotEqual(default, log.ProcessedAt);
    }
}
