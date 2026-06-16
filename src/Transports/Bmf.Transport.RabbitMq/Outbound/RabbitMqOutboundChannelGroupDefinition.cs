using System;

namespace Bmf.Transport.RabbitMq.Outbound;

public sealed record RabbitMqOutboundChannelGroupDefinition(
    string Name,
    int MaximumChannelCount,
    RabbitMqPublisherConfirmMode? PublisherConfirmMode = null,
    TimeSpan? PublisherConfirmTimeout = null
)
{
    public const string ReservedImplicitNamePrefix = "$implicit:";
}
