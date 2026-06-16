using System;
using Bmf.Abstractions;

namespace Bmf.Core.Tests.Messaging.TestSupport;

public record BaseSampleMessage(string Value) : ICloudEvent
{
    Guid ICloudEvent.Id { get; } = BmfUuid.NewId();

    DateTimeOffset ICloudEvent.Time { get; } = DateTimeOffset.UtcNow;

    string? ICloudEvent.Subject => null;
}
