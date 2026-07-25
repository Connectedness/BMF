using System;
using System.Collections.Generic;
using BrilliantMessaging.Core.Messaging;
using BrilliantMessaging.GuardClauses;

namespace BrilliantMessaging.Transport.Nats;

/// <summary>
/// Fluent builder for a JetStream stream declaration.
/// </summary>
public sealed class NatsStreamBuilder : IBuildable<NatsStreamDefinition>
{
    private readonly string _name;
    private readonly List<string> _subjects = [];
    private TimeSpan? _duplicateWindow;
    private TimeSpan? _maxAge;
    private int? _maxMessageSize;
    private int _replicas = 1;
    private NatsStreamRetention _retention = NatsStreamRetention.Limits;
    private NatsStreamStorage _storage = NatsStreamStorage.File;

    /// <summary>
    /// Initializes a new instance of the <see cref="NatsStreamBuilder" /> class.
    /// </summary>
    public NatsStreamBuilder(string name) => _name = name.MustNotBeNullOrWhiteSpace();

    /// <inheritdoc />
    NatsStreamDefinition IBuildable<NatsStreamDefinition>.Build()
    {
        return new NatsStreamDefinition(
            _name,
            _subjects.AsReadOnly(),
            _duplicateWindow,
            _maxAge,
            _maxMessageSize,
            _storage,
            _retention,
            _replicas
        );
    }

    /// <summary>
    /// Adds a NATS subject pattern. Stream patterns may include <c>*</c> and <c>&gt;</c> wildcards.
    /// </summary>
    public NatsStreamBuilder Subject(string subjectPattern)
    {
        _subjects.Add(subjectPattern.MustNotBeNullOrWhiteSpace());
        return this;
    }

    /// <summary>
    /// Configures the JetStream duplicate window used with NATS message-id deduplication.
    /// </summary>
    public NatsStreamBuilder DuplicateWindow(TimeSpan duplicateWindow)
    {
        duplicateWindow.MustBePositive();

        _duplicateWindow = duplicateWindow;
        return this;
    }

    /// <summary>
    /// Configures the stream maximum message age.
    /// </summary>
    public NatsStreamBuilder MaxAge(TimeSpan maxAge)
    {
        maxAge.MustBePositive();

        _maxAge = maxAge;
        return this;
    }

    /// <summary>
    /// Configures the stream maximum message size in bytes. NATS defaults to 1 MB when no server override exists.
    /// </summary>
    public NatsStreamBuilder MaxMessageSize(int bytes)
    {
        bytes.MustBePositive();

        _maxMessageSize = bytes;
        return this;
    }

    /// <summary>
    /// Configures the stream storage policy.
    /// </summary>
    public NatsStreamBuilder Storage(NatsStreamStorage storage)
    {
        storage.MustBeValidEnumValue();

        _storage = storage;
        return this;
    }

    /// <summary>
    /// Configures the stream retention policy.
    /// </summary>
    public NatsStreamBuilder Retention(NatsStreamRetention retention)
    {
        retention.MustBeValidEnumValue();

        _retention = retention;
        return this;
    }

    /// <summary>
    /// Configures the stream replica count from one through five.
    /// </summary>
    public NatsStreamBuilder Replicas(int replicas)
    {
        _replicas = replicas.MustBeIn(
            new Range<int>(
                NatsTopologyBuilderDefaults.MinimumStreamReplicas,
                NatsTopologyBuilderDefaults.MaximumStreamReplicas
            )
        );
        return this;
    }
}
