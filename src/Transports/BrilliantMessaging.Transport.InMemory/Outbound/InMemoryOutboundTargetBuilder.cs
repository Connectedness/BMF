using System;
using BrilliantMessaging.Core.Messaging;
using BrilliantMessaging.Core.Messaging.Outbound;
using BrilliantMessaging.GuardClauses;

namespace BrilliantMessaging.Transport.InMemory.Outbound;

/// <summary>
/// Fluent builder for an in-memory outbound target for messages of type <typeparamref name="TMessage" />. It
/// captures the declared topic and an optional serializer, then compiles them into an
/// <see cref="InMemoryOutboundTargetDefinition" />.
/// </summary>
/// <typeparam name="TMessage">The message type the target publishes.</typeparam>
public sealed class InMemoryOutboundTargetBuilder<TMessage> : IBuildable<InMemoryOutboundTargetDefinition>
{
    private readonly string? _targetName;
    private Type? _serializerType;
    private string? _topic;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryOutboundTargetBuilder{TMessage}" /> class.
    /// </summary>
    /// <param name="targetName">The optional name of the target the builder compiles.</param>
    public InMemoryOutboundTargetBuilder(string? targetName = null)
    {
        _targetName = targetName;
    }

    /// <inheritdoc />
    InMemoryOutboundTargetDefinition IBuildable<InMemoryOutboundTargetDefinition>.Build()
    {
        var topic = _topic.MustNotBeNull(
            static () => new InvalidOperationException(
                "An in-memory outbound target must select a topic with ToTopic(...)."
            )
        );
        return new InMemoryOutboundTargetDefinition(typeof(TMessage), topic, _targetName, _serializerType);
    }

    /// <summary>
    /// Routes published messages to the declared topic.
    /// </summary>
    /// <param name="topic">The declared topic to publish to.</param>
    /// <returns>The same builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="topic" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.EmptyStringException">
    /// Thrown when <paramref name="topic" /> is empty.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.WhiteSpaceStringException">
    /// Thrown when <paramref name="topic" /> contains only whitespace.
    /// </exception>
    public InMemoryOutboundTargetBuilder<TMessage> ToTopic(string topic)
    {
        topic.MustNotBeNullOrWhiteSpace();

        _topic = topic;
        return this;
    }

    /// <summary>
    /// Overrides the serializer used by the target with <typeparamref name="TSerializer" /> instead of the
    /// framework default.
    /// </summary>
    /// <typeparam name="TSerializer">The serializer type to use.</typeparam>
    /// <returns>The same builder for chaining.</returns>
    public InMemoryOutboundTargetBuilder<TMessage> WithSerializer<TSerializer>()
        where TSerializer : class, IMessageSerializer
    {
        _serializerType = typeof(TSerializer);
        return this;
    }
}
