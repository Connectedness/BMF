using System;
using BrilliantMessaging.GuardClauses;

namespace BrilliantMessaging.Core.Messaging.Outbound;

/// <summary>
/// Thrown when a required CloudEvents attribute is missing or invalid when publishing a message — for example a
/// message that neither implements <c>ICloudEvent</c> nor supplies explicit metadata. The message includes
/// instructions for supplying the attribute.
/// </summary>
public sealed class CloudEventMetadataException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudEventMetadataException" /> class.
    /// </summary>
    /// <param name="attributeName">The name of the missing or invalid CloudEvents attribute.</param>
    /// <param name="supplyInstructions">Guidance on how to supply the attribute, appended to the message.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="attributeName" /> or <paramref name="supplyInstructions" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.EmptyStringException">
    /// Thrown when <paramref name="attributeName" /> or <paramref name="supplyInstructions" /> is empty.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.WhiteSpaceStringException">
    /// Thrown when <paramref name="attributeName" /> or <paramref name="supplyInstructions" /> contains only
    /// whitespace.
    /// </exception>
    public CloudEventMetadataException(string attributeName, string supplyInstructions)
        : base(
            $"CloudEvents attribute '{attributeName.MustNotBeNullOrWhiteSpace()}' is missing or invalid. {supplyInstructions.MustNotBeNullOrWhiteSpace()}"
        )
    {
        AttributeName = attributeName;
    }

    /// <summary>
    /// Gets the name of the missing or invalid CloudEvents attribute.
    /// </summary>
    public string AttributeName { get; }
}
