using System.Collections.Generic;

namespace Usf.Transport.RabbitMq;

public sealed record RabbitMqExchangeBindingDefinition(
    string SourceExchangeName,
    string DestinationExchangeName,
    string RoutingKey,
    RabbitMqBindingMode BindingMode,
    IReadOnlyDictionary<string, object?> Arguments
) : RabbitMqBindingDefinition(SourceExchangeName, RoutingKey, BindingMode, Arguments);
