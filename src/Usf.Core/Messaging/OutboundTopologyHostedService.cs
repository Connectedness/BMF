using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Usf.Core.Messaging;

public sealed class OutboundTopologyHostedService : IHostedService
{
    private readonly IEnumerable<IOutboundTopologyProvisioner> _topologyProvisioners;
    private readonly IEnumerable<IOutboundTopologyValidator> _topologyValidators;

    public OutboundTopologyHostedService(
        IEnumerable<IOutboundTopologyValidator> topologyValidators,
        IEnumerable<IOutboundTopologyProvisioner> topologyProvisioners
    )
    {
        _topologyValidators = topologyValidators ?? throw new ArgumentNullException(nameof(topologyValidators));
        _topologyProvisioners = topologyProvisioners ?? throw new ArgumentNullException(nameof(topologyProvisioners));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var topologyValidator in _topologyValidators)
        {
            topologyValidator.Validate();
        }

        foreach (var topologyProvisioner in _topologyProvisioners)
        {
            await topologyProvisioner.ProvisionAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
