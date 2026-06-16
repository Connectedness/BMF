using System;
using System.Threading;
using System.Threading.Tasks;
using Bmf.Core.Messaging;
using Bmf.Core.Messaging.Outbound;

namespace Bmf.Core.Tests.Messaging.TestSupport;

public sealed class ThrowingSerializer : IMessageSerializer
{
    private readonly Exception _exception;

    public ThrowingSerializer(Exception exception)
    {
        _exception = exception;
    }

    public ValueTask<CloudEventEnvelope> SerializeAsync<T>(
        T message,
        in CloudEventMetadata metadata,
        string? type,
        string? dataSchema,
        CancellationToken cancellationToken = default
    )
    {
        throw _exception;
    }
}
