using System;

namespace Bmf.Core.Messaging;

public sealed class MessageContractNotRegisteredException : Exception
{
    public MessageContractNotRegisteredException(Type messageType)
        : base($"No canonical CloudEvents discriminator is registered for message type '{messageType}'.")
    {
        MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
    }

    public Type MessageType { get; }
}
