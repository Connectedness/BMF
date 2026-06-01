using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Usf.Core.Messaging.Serialization;

namespace Usf.Core.Messaging;

public static class CloudEventsServiceCollectionExtensions
{
    public static IServiceCollection AddCloudEvents(
        this IServiceCollection services,
        Action<CloudEventsOptions> configureOptions,
        Action<MessageContractRegistryBuilder> configureContracts
    )
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions is null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        if (configureContracts is null)
        {
            throw new ArgumentNullException(nameof(configureContracts));
        }

        CloudEventsOptions options = new ();
        configureOptions(options);
        MessageContractRegistryBuilder registryBuilder = new ();
        configureContracts(registryBuilder);
        var registry = registryBuilder.Build();

        services.TryAddSingleton(options);
        services.TryAddSingleton(registry);
        services.TryAddSingleton<IPayloadCodec, Utf8JsonPayloadCodec>();
        services.TryAddSingleton<CloudEventMessageSerializer>();
        services.TryAddSingleton<IMessageSerializer>(
            static serviceProvider => serviceProvider.GetRequiredService<CloudEventMessageSerializer>()
        );
        services.TryAddSingleton<IOutboundTopologyValidator, MessageContractOutboundTopologyValidator>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, CloudEventsOptionsHostedService>());

        return services;
    }
}
