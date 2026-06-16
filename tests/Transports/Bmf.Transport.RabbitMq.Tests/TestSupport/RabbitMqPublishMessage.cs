using System;
using Bmf.Abstractions;

namespace Bmf.Transport.RabbitMq.Tests.TestSupport;

public sealed record RabbitMqPublishMessage(int Id, string Name) : ICloudEvent
{
    Guid ICloudEvent.Id { get; } = BmfUuid.NewId();

    DateTimeOffset ICloudEvent.Time { get; } = DateTimeOffset.UtcNow;

    string? ICloudEvent.Subject => null;
}
