using System.Collections.Generic;

namespace Usf.Core.Messaging.Outbound;

public readonly record struct SerializedMessage(
    byte[] Body,
    string? ContentType,
    string? ContentEncoding,
    IReadOnlyDictionary<string, string?> Headers,
    string? MessageId,
    string? CorrelationId
);
