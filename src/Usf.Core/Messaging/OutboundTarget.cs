using System;
using System.Threading;
using System.Threading.Tasks;
using Usf.Core.Messaging.Errors;

namespace Usf.Core.Messaging;

public abstract class OutboundTarget
{
    protected OutboundTarget(string name, string transportName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(transportName))
        {
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(transportName));
        }

        Name = name;
        TransportName = transportName;
    }

    public virtual Type? MessageType => null;

    public string Name { get; }

    public string TransportName { get; }

    public abstract Task PublishSerializedAsync(
        SerializedMessage message,
        CancellationToken cancellationToken = default
    );
}

public abstract class OutboundTarget<T> : OutboundTarget
{
    protected OutboundTarget(string name, string transportName, IMessageSerializer serializer)
        : base(name, transportName)
    {
        Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public sealed override Type MessageType => typeof(T);

    protected IMessageSerializer Serializer { get; }

    public virtual async Task PublishAsync(T message, CancellationToken cancellationToken = default)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        SerializedMessage serializedMessage;

        try
        {
            serializedMessage = await Serializer.SerializeAsync(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException &&
                                          exception is not MessageSerializationException)
        {
            throw new MessageSerializationException(typeof(T), exception);
        }

        await PublishTypedSerializedAsync(message, serializedMessage, cancellationToken).ConfigureAwait(false);
    }

    protected virtual Task PublishTypedSerializedAsync(
        T message,
        SerializedMessage serializedMessage,
        CancellationToken cancellationToken
    )
    {
        return PublishSerializedAsync(serializedMessage, cancellationToken);
    }
}
