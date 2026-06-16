using System;
using System.Threading;
using System.Threading.Tasks;
using Bmf.Abstractions;
using Bmf.Core.Messaging;
using Bmf.Core.Messaging.Outbound;

namespace Bmf.Transport.RabbitMq.Outbound;

public abstract class RabbitMqRoutingKeyOutboundTarget<TMessage>
    : RabbitMqOutboundTarget<TMessage>, IOutboundRoutableTarget<TMessage>
{
    private readonly string? _constantRoutingKey;
    private readonly Func<TMessage, string>? _routingKeyFactory;

    protected RabbitMqRoutingKeyOutboundTarget(
        string name,
        IMessageSerializer serializer,
        IMessageContractRegistry messageContractRegistry,
        string topologyName,
        RabbitMqOutboundChannelGroup channelGroup,
        string exchangeName,
        bool isMandatory,
        string? constantRoutingKey,
        Func<TMessage, string>? routingKeyFactory
    )
        : base(name, serializer, messageContractRegistry, topologyName, channelGroup, exchangeName, isMandatory)
    {
        if (constantRoutingKey is null && routingKeyFactory is null)
        {
            throw new ArgumentException("A routing-key target must provide a constant key or a key factory.");
        }

        if (constantRoutingKey is not null && routingKeyFactory is not null)
        {
            throw new ArgumentException("A routing-key target cannot provide both a constant key and a key factory.");
        }

        _constantRoutingKey = constantRoutingKey;
        _routingKeyFactory = routingKeyFactory;
    }

    public Task PublishAsync(
        TMessage message,
        string routingKey,
        CancellationToken cancellationToken = default
    )
    {
        EnsureRoutingKey(routingKey);

        if (message is not ICloudEvent cloudEvent)
        {
            throw new CloudEventMetadataException(
                CloudEventAttributeNames.Id,
                "Implement ICloudEvent or derive from BaseCloudEvent, or call PublishAsync with explicit CloudEventMetadata."
            );
        }

        var metadata = CloudEventMetadata.From(cloudEvent);
        return PublishCoreAsync(message, metadata, type: null, dataSchema: null, routingKey, cancellationToken);
    }

    public Task PublishAsync(
        TMessage message,
        in CloudEventMetadata metadata,
        string routingKey,
        CancellationToken cancellationToken = default
    )
    {
        EnsureRoutingKey(routingKey);
        return PublishCoreAsync(message, metadata, type: null, dataSchema: null, routingKey, cancellationToken);
    }

    public Task PublishAsync(
        TMessage message,
        in CloudEventMetadata metadata,
        string type,
        string? dataSchema,
        string routingKey,
        CancellationToken cancellationToken = default
    )
    {
        EnsureRoutingKey(routingKey);
        return PublishCoreAsync(message, metadata, type, dataSchema, routingKey, cancellationToken);
    }

    protected override string GetRawRoutingKey()
    {
        return _constantRoutingKey ??
               throw new InvalidOperationException(
                   "Raw publishing is not supported for RabbitMQ outbound targets with message-derived routing keys."
               );
    }

    protected override string GetRoutingKey(TMessage message)
    {
        if (_constantRoutingKey is not null)
        {
            return _constantRoutingKey;
        }

        return _routingKeyFactory!(message) ??
               throw new InvalidOperationException("The RabbitMQ routing key factory returned null.");
    }

    private static void EnsureRoutingKey(string routingKey)
    {
        if (string.IsNullOrWhiteSpace(routingKey))
        {
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(routingKey));
        }
    }
}
