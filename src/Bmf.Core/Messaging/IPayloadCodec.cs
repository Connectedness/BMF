using System;

namespace Bmf.Core.Messaging;

/// <summary>
/// Encodes and decodes message payloads.
/// </summary>
public interface IPayloadCodec
{
    EncodedPayload Encode<T>(T message);

    object? Decode(ReadOnlyMemory<byte> data, Type messageType);
}
