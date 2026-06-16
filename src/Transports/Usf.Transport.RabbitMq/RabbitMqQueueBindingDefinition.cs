using System.Collections.Generic;

namespace Usf.Transport.RabbitMq;

public sealed record RabbitMqQueueBindingDefinition(
    string SourceExchangeName,
    string QueueName,
    string RoutingKey,
    RabbitMqBindingMode BindingMode,
    IReadOnlyDictionary<string, object?> Arguments
) : RabbitMqBindingDefinition(SourceExchangeName, RoutingKey, BindingMode, Arguments);
