using System.Collections.Generic;

namespace Bmf.Transport.RabbitMq;

public abstract record RabbitMqBindingDefinition(
    string SourceExchangeName,
    string RoutingKey,
    RabbitMqBindingMode BindingMode,
    IReadOnlyDictionary<string, object?> Arguments
);
