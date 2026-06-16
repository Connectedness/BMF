using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Usf.Transport.RabbitMq.Outbound;

public sealed class RabbitMqOutboundChannelGroup : IAsyncDisposable, IDisposable
{
    private readonly IRabbitMqChannelPool _channelPool;

    public RabbitMqOutboundChannelGroup(
        string name,
        int maximumChannelCount,
        Func<CancellationToken, Task<IChannel>> channelFactory,
        RabbitMqPublisherConfirmMode publisherConfirmMode = RabbitMqPublisherConfirmDefaults.Mode,
        TimeSpan? publisherConfirmTimeout = null
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The value cannot be null or whitespace.", nameof(name));
        }

        if (maximumChannelCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumChannelCount),
                maximumChannelCount,
                "The value must be greater than zero."
            );
        }

        if (!Enum.IsDefined(typeof(RabbitMqPublisherConfirmMode), publisherConfirmMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(publisherConfirmMode),
                publisherConfirmMode,
                "Unsupported publisher confirm mode."
            );
        }

        var resolvedPublisherConfirmTimeout = publisherConfirmTimeout ?? RabbitMqPublisherConfirmDefaults.Timeout;

        if (!RabbitMqPublisherConfirmDefaults.IsValidTimeout(resolvedPublisherConfirmTimeout))
        {
            throw new ArgumentOutOfRangeException(
                nameof(publisherConfirmTimeout),
                resolvedPublisherConfirmTimeout,
                "The value must be finite and greater than zero."
            );
        }

        Name = name;
        MaximumChannelCount = maximumChannelCount;
        PublisherConfirmMode = publisherConfirmMode;
        PublisherConfirmTimeout = resolvedPublisherConfirmTimeout;
        _channelPool = new DefaultRabbitMqChannelPool(
            maximumChannelCount,
            channelFactory ?? throw new ArgumentNullException(nameof(channelFactory))
        );
    }

    public string Name { get; }

    public int MaximumChannelCount { get; }

    public RabbitMqPublisherConfirmMode PublisherConfirmMode { get; }

    public TimeSpan PublisherConfirmTimeout { get; }

    public ValueTask DisposeAsync()
    {
        return _channelPool.DisposeAsync();
    }

    public void Dispose()
    {
        _channelPool.Dispose();
    }

    public ValueTask<RabbitMqChannelLease> AcquireAsync(CancellationToken cancellationToken = default)
    {
        return _channelPool.AcquireAsync(cancellationToken);
    }
}
