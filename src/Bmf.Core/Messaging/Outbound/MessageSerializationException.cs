using System;

namespace Bmf.Core.Messaging.Outbound;

public sealed class MessageSerializationException : Exception
{
    public MessageSerializationException(Type messageType, Exception innerException)
        : base($"Serialization failed for message type '{messageType}'.", innerException)
    {
        MessageType = messageType;
    }

    public Type MessageType { get; }
}
