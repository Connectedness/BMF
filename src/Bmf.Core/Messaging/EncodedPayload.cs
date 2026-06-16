namespace Bmf.Core.Messaging;

public readonly record struct EncodedPayload(byte[] Data, string DataContentType);
