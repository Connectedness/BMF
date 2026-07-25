using System;
using System.Threading;
using System.Threading.Tasks;
using BrilliantMessaging.GuardClauses;

namespace BrilliantMessaging.Core.Messaging.Inbound;

/// <summary>
/// An inbound message inspector that returns a pre-resolved discriminator and message type when a predicate matches
/// the transport message.
/// </summary>
public sealed class PredicateInboundMessageInspector : IInboundMessageInspector
{
    private readonly string _discriminator;
    private readonly Type _messageType;
    private readonly Func<TransportMessage, bool> _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateInboundMessageInspector" /> class.
    /// </summary>
    /// <param name="predicate">The predicate that decides whether the inspector recognizes the delivery.</param>
    /// <param name="discriminator">The discriminator returned when the predicate matches.</param>
    /// <param name="messageType">The message type returned when the predicate matches.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate" /> or <paramref name="messageType" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="discriminator" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.EmptyStringException">
    /// Thrown when <paramref name="discriminator" /> is empty.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.WhiteSpaceStringException">
    /// Thrown when <paramref name="discriminator" /> contains only whitespace.
    /// </exception>
    public PredicateInboundMessageInspector(
        Func<TransportMessage, bool> predicate,
        string discriminator,
        Type messageType
    )
    {
        _predicate = predicate.MustNotBeNull();
        _discriminator = discriminator.MustNotBeNullOrWhiteSpace();
        _messageType = messageType.MustNotBeNull();
    }

    /// <inheritdoc />
    public ValueTask<InboundMessageInspectionResult?> InspectAsync(
        TransportMessage transportMessage,
        CancellationToken cancellationToken = default
    )
    {
        transportMessage.MustNotBeNull();

        return new ValueTask<InboundMessageInspectionResult?>(
            _predicate(transportMessage) ?
                new InboundMessageInspectionResult(_discriminator, _messageType) :
                null
        );
    }
}
