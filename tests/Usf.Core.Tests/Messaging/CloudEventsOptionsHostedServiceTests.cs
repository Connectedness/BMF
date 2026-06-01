using System.Threading.Tasks;
using FluentAssertions;
using Usf.Core.Messaging;
using Usf.Core.Messaging.Errors;
using Xunit;

namespace Usf.Core.Tests.Messaging;

public sealed class CloudEventsOptionsHostedServiceTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task StartAsync_RejectsMissingSource(string? source)
    {
        var hostedService = new CloudEventsOptionsHostedService(
            new CloudEventsOptions
            {
                Source = source
            }
        );

        var action = async () => await hostedService.StartAsync(TestContext.Current.CancellationToken);

        var exception = (await action.Should().ThrowAsync<CloudEventMetadataException>()).Which;
        exception.AttributeName.Should().Be("source");
    }

    [Fact]
    public async Task StartAsync_AcceptsUriReferenceSource()
    {
        var hostedService = new CloudEventsOptionsHostedService(
            new CloudEventsOptions
            {
                Source = "/applications/orders"
            }
        );

        await hostedService.StartAsync(TestContext.Current.CancellationToken);
    }
}
