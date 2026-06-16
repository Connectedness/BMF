using System;
using System.Collections.Generic;

namespace Usf.Transport.RabbitMq.Outbound;

public sealed record RabbitMqHeadersOutboundTargetDefinition(
    Type MessageType,
    string ExchangeName,
    string? ChannelGroupName,
    string? TargetName,
    Type? SerializerType,
    bool IsMandatory,
    IReadOnlyDictionary<string, object?> Headers
) : RabbitMqOutboundTargetDefinition(
    MessageType,
    ExchangeName,
    ChannelGroupName,
    TargetName,
    SerializerType,
    IsMandatory
);
