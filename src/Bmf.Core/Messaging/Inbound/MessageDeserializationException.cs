using System;

namespace Bmf.Core.Messaging.Inbound;

public sealed class MessageDeserializationException : Exception
{
    public MessageDeserializationException(Type messageType, Exception innerException)
        : base($"Deserialization failed for message type '{messageType}'.", innerException)
    {
        MessageType = messageType;
    }

    public Type MessageType { get; }
}
