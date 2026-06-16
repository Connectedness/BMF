using System;
using Bmf.Abstractions;

namespace Bmf.Transport.RabbitMq.Tests.TestSupport;

public sealed record RabbitMqAuditMessage(int Id, string EventName) : ICloudEvent
{
    Guid ICloudEvent.Id { get; } = BmfUuid.NewId();

    DateTimeOffset ICloudEvent.Time { get; } = DateTimeOffset.UtcNow;

    string? ICloudEvent.Subject => null;
}
