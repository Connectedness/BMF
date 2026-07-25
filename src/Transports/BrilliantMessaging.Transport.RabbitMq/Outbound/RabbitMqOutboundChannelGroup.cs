using System;
using System.Threading;
using System.Threading.Tasks;
using BrilliantMessaging.GuardClauses;
using RabbitMQ.Client;

namespace BrilliantMessaging.Transport.RabbitMq.Outbound;

/// <summary>
/// A pool of publish channels that share a publisher-confirm mode and timeout. Outbound targets acquire a
/// channel from their group for each publish, so the group's maximum channel count bounds publish concurrency.
/// </summary>
public sealed class RabbitMqOutboundChannelGroup : IAsyncDisposable, IDisposable
{
    private readonly IRabbitMqChannelPool _channelPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqOutboundChannelGroup" /> class.
    /// </summary>
    /// <param name="name">The group name.</param>
    /// <param name="maximumChannelCount">The maximum number of channels the group may open; must be greater than zero.</param>
    /// <param name="channelFactory">A factory that opens a new channel for the group.</param>
    /// <param name="publisherConfirmMode">The publisher-confirm mode for channels in the group.</param>
    /// <param name="publisherConfirmTimeout">The bounded wait for publisher confirmations, or <see langword="null" /> for the default.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.EmptyStringException">
    /// Thrown when <paramref name="name" /> is empty.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.WhiteSpaceStringException">
    /// Thrown when <paramref name="name" /> contains only whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maximumChannelCount" /> is less than one or the confirm timeout is out of
    /// range.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.EnumValueNotDefinedException">
    /// Thrown when <paramref name="publisherConfirmMode" /> is
    /// undefined.
    /// </exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="channelFactory" /> is <see langword="null" />.</exception>
    public RabbitMqOutboundChannelGroup(
        string name,
        int maximumChannelCount,
        Func<CancellationToken, Task<IChannel>> channelFactory,
        RabbitMqPublisherConfirmMode publisherConfirmMode = RabbitMqPublisherConfirmDefaults.Mode,
        TimeSpan? publisherConfirmTimeout = null
    )
    {
        name.MustNotBeNullOrWhiteSpace();

        maximumChannelCount.MustBePositive();

        publisherConfirmMode.MustBeValidEnumValue();

        var resolvedPublisherConfirmTimeout = (publisherConfirmTimeout ?? RabbitMqPublisherConfirmDefaults.Timeout)
           .MustBeIn(
                new Range<TimeSpan>(
                    TimeSpan.Zero,
                    TimeSpan.FromMilliseconds(uint.MaxValue - 1d),
                    isFromInclusive: false
                ),
                nameof(publisherConfirmTimeout)
            );

        Name = name;
        MaximumChannelCount = maximumChannelCount;
        PublisherConfirmMode = publisherConfirmMode;
        PublisherConfirmTimeout = resolvedPublisherConfirmTimeout;
        _channelPool = new DefaultRabbitMqChannelPool(
            maximumChannelCount,
            channelFactory.MustNotBeNull()
        );
    }

    /// <summary>
    /// Gets the group name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the maximum number of channels the group may open.
    /// </summary>
    public int MaximumChannelCount { get; }

    /// <summary>
    /// Gets the publisher-confirm mode for channels in the group.
    /// </summary>
    public RabbitMqPublisherConfirmMode PublisherConfirmMode { get; }

    /// <summary>
    /// Gets the bounded wait applied to publisher confirmations.
    /// </summary>
    public TimeSpan PublisherConfirmTimeout { get; }

    /// <summary>
    /// Asynchronously disposes the group and its channel pool.
    /// </summary>
    /// <returns>A task that completes once the pool is disposed.</returns>
    public ValueTask DisposeAsync()
    {
        return _channelPool.DisposeAsync();
    }

    /// <summary>
    /// Disposes the group and its channel pool.
    /// </summary>
    public void Dispose()
    {
        _channelPool.Dispose();
    }

    /// <summary>
    /// Acquires a publish channel from the group's pool.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for a channel.</param>
    /// <returns>A lease over the acquired channel.</returns>
    public ValueTask<RabbitMqChannelLease> AcquireAsync(CancellationToken cancellationToken = default)
    {
        return _channelPool.AcquireAsync(cancellationToken);
    }
}
