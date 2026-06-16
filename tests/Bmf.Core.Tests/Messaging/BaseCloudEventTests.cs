using System;
using FluentAssertions;
using Bmf.Abstractions;
using Xunit;

namespace Bmf.Core.Tests.Messaging;

public sealed class BaseCloudEventTests
{
    [Fact]
    public void Constructor_CreatesStableNonEmptyIdAndUtcTime()
    {
        var message = new TestCloudEvent();
        var id = message.Id;
        var time = message.Time;

        id.Should().NotBe(Guid.Empty);
        message.Id.Should().Be(id);
        time.Offset.Should().Be(TimeSpan.Zero);
        message.Time.Should().Be(time);
    }

    [Fact]
    public void NewId_CreatesDistinctIdentifiers()
    {
        BmfUuid.NewId().Should().NotBe(BmfUuid.NewId());
    }

    private sealed record TestCloudEvent : BaseCloudEvent;
}
