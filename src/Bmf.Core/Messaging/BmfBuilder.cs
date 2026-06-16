using System;
using Microsoft.Extensions.DependencyInjection;
using Bmf.Core.Messaging.Outbound;

namespace Bmf.Core.Messaging;

public sealed class BmfBuilder
{
    public BmfBuilder(
        IServiceCollection services,
        MessageContractRegistryBuilder messageContracts,
        TopologyRegistrationCatalog topologies
    )
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        MessageContracts = messageContracts ?? throw new ArgumentNullException(nameof(messageContracts));
        Topologies = topologies ?? throw new ArgumentNullException(nameof(topologies));
    }

    public IServiceCollection Services { get; }

    public MessageContractRegistryBuilder MessageContracts { get; }

    public TopologyRegistrationCatalog Topologies { get; }

    public BmfBuilder MapMessageContracts(Action<MessageContractRegistryBuilder> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        configure(MessageContracts);
        return this;
    }

    public BmfBuilder UseCloudEvents(Action<CloudEventsOptions> configure)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        Services.Configure(configure);
        return this;
    }
}
