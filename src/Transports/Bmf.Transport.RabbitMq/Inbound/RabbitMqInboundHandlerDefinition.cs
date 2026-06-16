using System;
using Bmf.Core.Messaging.Inbound;

namespace Bmf.Transport.RabbitMq.Inbound;

public sealed record RabbitMqInboundHandlerDefinition(
    string? EndpointName,
    Type MessageType,
    Type HandlerType,
    MessageDelegate HandlerInvocation,
    Type DeserializerType,
    MessageAckMode AckMode
);
