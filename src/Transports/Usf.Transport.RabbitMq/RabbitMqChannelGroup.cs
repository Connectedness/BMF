using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Usf.Transport.RabbitMq;

public sealed class RabbitMqChannelGroup : IAsyncDisposable, IDisposable
{
    private readonly IRabbitMqChannelPool _channelPool;

    public RabbitMqChannelGroup(
        string name,
        int maximumChannelCount,
        Func<CancellationToken, Task<IChannel>> channelFactory
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

        Name = name;
        MaximumChannelCount = maximumChannelCount;
        _channelPool = new DefaultRabbitMqChannelPool(
            maximumChannelCount,
            channelFactory ?? throw new ArgumentNullException(nameof(channelFactory))
        );
    }

    public string Name { get; }

    public int MaximumChannelCount { get; }

    public ValueTask<RabbitMqChannelLease> AcquireAsync(CancellationToken cancellationToken = default)
    {
        return _channelPool.AcquireAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return _channelPool.DisposeAsync();
    }

    public void Dispose()
    {
        _channelPool.Dispose();
    }
}
