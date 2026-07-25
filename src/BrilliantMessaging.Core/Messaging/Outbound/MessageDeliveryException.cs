using System;
using BrilliantMessaging.GuardClauses;

namespace BrilliantMessaging.Core.Messaging.Outbound;

/// <summary>
/// Thrown when a transport accepts a publish but the broker subsequently fails to deliver it — for example a
/// negative acknowledgement (nack), an unroutable return, or a confirm timeout. The specific cause is given by
/// <see cref="Reason" />.
/// </summary>
public sealed class MessageDeliveryException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageDeliveryException" /> class.
    /// </summary>
    /// <param name="targetName">The name of the target the delivery failed for.</param>
    /// <param name="reason">The reason delivery failed.</param>
    /// <param name="innerException">
    /// The underlying exception. Required for every reason except
    /// <see cref="MessageDeliveryFailureReason.Timeout" />, which must not provide one.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetName" /> is <see langword="null" />.</exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.EmptyStringException">Thrown when <paramref name="targetName" /> is empty.</exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.WhiteSpaceStringException">
    /// Thrown when <paramref name="targetName" /> contains only
    /// whitespace.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="innerException" /> is inconsistent with <paramref name="reason" />.</exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.EnumValueNotDefinedException">
    /// Thrown when <paramref name="reason" /> is not a defined
    /// value.
    /// </exception>
    public MessageDeliveryException(
        string targetName,
        MessageDeliveryFailureReason reason,
        Exception? innerException = null
    )
        : base(CreateMessage(targetName, reason, innerException), innerException)
    {
        TargetName = targetName;
        Reason = reason;
    }

    /// <summary>
    /// Gets the name of the target the delivery failed for.
    /// </summary>
    public string TargetName { get; }

    /// <summary>
    /// Gets the reason the delivery failed.
    /// </summary>
    public MessageDeliveryFailureReason Reason { get; }

    private static string CreateMessage(
        string targetName,
        MessageDeliveryFailureReason reason,
        Exception? innerException
    )
    {
        targetName.MustNotBeNullOrWhiteSpace();
        reason.MustBeValidEnumValue();
        Check.InvalidArgument(
            reason == MessageDeliveryFailureReason.Timeout && innerException is not null,
            nameof(innerException),
            "A delivery timeout cannot provide an inner exception."
        );
        Check.InvalidArgument(
            reason != MessageDeliveryFailureReason.Timeout && innerException is null,
            nameof(innerException),
            "A delivery failure other than timeout must provide an inner exception."
        );

        return $"Delivery failed for outbound target '{targetName}' with reason '{reason}'.";
    }
}
