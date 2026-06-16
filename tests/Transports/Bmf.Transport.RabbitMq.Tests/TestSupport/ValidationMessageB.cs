using System;
using Bmf.Abstractions;

namespace Bmf.Transport.RabbitMq.Tests.TestSupport;

public sealed record ValidationMessageB(string Value) : ICloudEvent
{
    Guid ICloudEvent.Id { get; } = BmfUuid.NewId();

    DateTimeOffset ICloudEvent.Time { get; } = DateTimeOffset.UtcNow;

    string? ICloudEvent.Subject => null;
}
