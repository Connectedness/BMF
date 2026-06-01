using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Usf.Core.Messaging;

public sealed class CloudEventsOptionsHostedService : IHostedService
{
    private readonly CloudEventsOptions _options;

    public CloudEventsOptionsHostedService(CloudEventsOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CloudEventsOptionsValidation.GetRequiredSource(_options);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
