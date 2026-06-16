using System;

namespace Usf.Transport.RabbitMq.Outbound;

public sealed record RabbitMqTopicOutboundTargetDefinition(
    Type MessageType,
    string ExchangeName,
    string? ChannelGroupName,
    string? TargetName,
    Type? SerializerType,
    bool IsMandatory,
    string? RoutingKey,
    Delegate? RoutingKeyFactory
) : RabbitMqRoutingKeyOutboundTargetDefinition(
    MessageType,
    ExchangeName,
    ChannelGroupName,
    TargetName,
    SerializerType,
    IsMandatory,
    RoutingKey,
    RoutingKeyFactory
);
