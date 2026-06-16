using System;
using Usf.Core.Messaging.Inbound;

namespace Usf.Transport.RabbitMq.Inbound;

public sealed record RabbitMqInboundHandlerDefinition(
    string? EndpointName,
    Type MessageType,
    Type HandlerType,
    MessageDelegate HandlerInvocation,
    Type DeserializerType,
    MessageAckMode AckMode
);
