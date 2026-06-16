namespace Usf.Transport.RabbitMq.Inbound;

public sealed record RabbitMqInboundChannelGroupDefinition(
    string Name,
    int MaximumChannelCount,
    ushort PrefetchCount,
    ushort ConsumerDispatchConcurrency
)
{
    public const string ReservedImplicitNamePrefix = "$implicit:";
}
