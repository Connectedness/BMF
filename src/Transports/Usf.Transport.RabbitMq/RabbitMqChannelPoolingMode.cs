namespace Usf.Transport.RabbitMq;

public enum RabbitMqChannelPoolingMode
{
    /// <summary>
    /// Each compiled target owns its own bounded channel pool. With the default
    /// <see cref="RabbitMqPublishingConfiguration.MaxChannelsPerTarget" /> of 1, publishes against a single target are
    /// serialized through one channel, which preserves per-target publish ordering. Raising the per-target maximum
    /// allows concurrent publishes per target and therefore no longer guarantees per-target ordering.
    /// </summary>
    PerTarget = 0,

    /// <summary>
    /// Uses a single connection-level channel pool for all targets. This reduces channel count, but per-target
    /// publish ordering is not guaranteed.
    /// </summary>
    Shared = 1
}
