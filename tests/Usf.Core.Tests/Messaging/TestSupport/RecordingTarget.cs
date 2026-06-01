using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Usf.Core.Messaging;

namespace Usf.Core.Tests.Messaging.TestSupport;

public sealed class RecordingTarget<TMessage> : OutboundTarget<TMessage>
{
    public RecordingTarget(string name, IMessageSerializer serializer)
        : base(name, "test", serializer) { }

    public List<TMessage> Messages { get; } = [];

    public List<CloudEventEnvelope> CloudEventEnvelopes { get; } = [];

    public List<SerializedMessage> SerializedMessages { get; } = [];

    public override Task PublishSerializedAsync(
        SerializedMessage message,
        CancellationToken cancellationToken = default
    )
    {
        SerializedMessages.Add(message);
        return Task.CompletedTask;
    }

    protected override Task PublishTypedCloudEventAsync(
        TMessage message,
        CloudEventEnvelope envelope,
        CancellationToken cancellationToken
    )
    {
        Messages.Add(message);
        CloudEventEnvelopes.Add(envelope);
        return Task.CompletedTask;
    }
}
