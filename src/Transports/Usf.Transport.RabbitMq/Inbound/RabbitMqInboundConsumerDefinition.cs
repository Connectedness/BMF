using System;
using System.Collections.Generic;

namespace Usf.Transport.RabbitMq.Inbound;

public sealed record RabbitMqInboundConsumerDefinition(
    string QueueName,
    Type InspectorType,
    string? ChannelGroupName,
    int ChannelCount,
    ushort PrefetchCount,
    ushort ConsumerDispatchConcurrency,
    bool CopyBody,
    IReadOnlyList<RabbitMqInboundHandlerDefinition> Handlers
);
