namespace Usf.Transport.RabbitMq.Configuration;

public sealed record RabbitMqChannelGroupDefinition(string Name, int MaximumChannelCount);
