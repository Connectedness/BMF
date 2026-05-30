using System;

namespace Usf.Transport.RabbitMq.Configuration;

public sealed record RabbitMqChannelGroupDefinition(
    string Name,
    int MaximumChannelCount,
    RabbitMqPublisherConfirmMode? PublisherConfirmMode = null,
    TimeSpan? PublisherConfirmTimeout = null
);
