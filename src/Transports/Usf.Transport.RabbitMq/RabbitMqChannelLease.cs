using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Usf.Transport.RabbitMq;

public readonly struct RabbitMqChannelLease : IAsyncDisposable
{
    private readonly IRabbitMqChannelPool? _pool;
    private readonly IChannel? _channel;

    public RabbitMqChannelLease(IRabbitMqChannelPool pool, IChannel channel, object? state = null, long token = 0)
    {
        _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        State = state;
        Token = token;
    }

    public IChannel Channel =>
        _channel ?? throw new ObjectDisposedException(nameof(RabbitMqChannelLease));

    public object? State { get; }

    public long Token { get; }

    public ValueTask DisposeAsync()
    {
        return _pool?.ReleaseAsync(in this) ?? default;
    }
}
