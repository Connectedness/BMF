using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Usf.Core.Messaging;

namespace Usf.Transport.RabbitMq;

public static class RabbitMqTransportModule
{
    public static IServiceCollection AddRabbitMqOutboundTopology(
        this IServiceCollection services,
        Action<RabbitMqOutboundTopologyBuilder> configure
    )
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var builder = new RabbitMqOutboundTopologyBuilder();
        configure(builder);
        var configuration = builder.Build();

        services.AddSingleton(configuration);
        services.AddSingleton<RabbitMqOutboundTopology>(
            static serviceProvider => RabbitMqOutboundTopologyCompiler.Compile(serviceProvider)
        );
        services.AddSingleton<IOutboundTopology>(
            static serviceProvider => serviceProvider.GetRequiredService<RabbitMqOutboundTopology>().OutboundTopology
        );
        services.AddSingleton<OutboundTopology>(
            static serviceProvider => serviceProvider.GetRequiredService<RabbitMqOutboundTopology>().OutboundTopology
        );
        services.AddSingleton<IOutboundTargetRegistry>(
            static serviceProvider => serviceProvider.GetRequiredService<IOutboundTopology>()
        );
        services.AddSingleton<IMessagePublisher, MessagePublisher>();
        services.AddSingleton<IOutboundTopologyProvisioner, RabbitMqOutboundTopologyProvisioner>();
        services.AddSingleton<IHostedService, OutboundTopologyHostedService>();

        return services;
    }
}
