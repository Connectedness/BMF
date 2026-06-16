using System;

namespace Usf.Transport.RabbitMq.Outbound;

public abstract record RabbitMqOutboundTargetDefinition(
    Type MessageType,
    string ExchangeName,
    string? ChannelGroupName,
    string? TargetName,
    Type? SerializerType,
    bool IsMandatory
);
