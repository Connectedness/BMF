using System.Collections.Generic;

namespace Bmf.Transport.RabbitMq;

public sealed record RabbitMqExchangeDefinition(
    string Name,
    string Type,
    RabbitMqDeclareMode DeclareMode,
    bool Durable,
    bool AutoDelete,
    IReadOnlyDictionary<string, object?> Arguments
);
