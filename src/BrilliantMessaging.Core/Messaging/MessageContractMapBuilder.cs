using System;
using BrilliantMessaging.GuardClauses;

namespace BrilliantMessaging.Core.Messaging;

/// <summary>
/// Refines a single message-contract mapping produced by <see cref="MessageContractRegistryBuilder.Map{T}" />
/// or <see cref="MessageContractRegistryBuilder.MapOutbound{T}" />, adding inbound aliases and a data schema.
/// </summary>
public sealed class MessageContractMapBuilder
{
    private readonly MessageContractRegistration _registration;

    internal MessageContractMapBuilder(MessageContractRegistration registration)
    {
        _registration = registration;
    }

    /// <summary>
    /// Registers an additional inbound discriminator (alias) that maps to the same message type, allowing the
    /// consumer to accept messages published under a legacy or alternative <c>type</c> value.
    /// </summary>
    /// <param name="discriminator">The alias discriminator to accept inbound.</param>
    /// <returns>The same <see cref="MessageContractMapBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="discriminator" /> is null or whitespace.</exception>
    public MessageContractMapBuilder WithInboundAlias(string discriminator)
    {
        _registration.InboundAliases.Add(discriminator.MustNotBeNullOrWhiteSpace());
        return this;
    }

    /// <summary>
    /// Sets the CloudEvents <c>dataschema</c> attribute attached to messages of this type.
    /// </summary>
    /// <param name="dataSchema">A URI-reference identifying the data schema.</param>
    /// <returns>The same <see cref="MessageContractMapBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataSchema" /> is <see langword="null" />.</exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.EmptyStringException">Thrown when <paramref name="dataSchema" /> is empty.</exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.WhiteSpaceStringException">
    /// Thrown when <paramref name="dataSchema" /> contains only
    /// whitespace.
    /// </exception>
    /// <exception cref="BrilliantMessaging.GuardClauses.Exceptions.InvalidUriException">
    /// Thrown when <paramref name="dataSchema" /> is not a valid
    /// URI-reference.
    /// </exception>
    public MessageContractMapBuilder WithDataSchema(string dataSchema)
    {
        _registration.DataSchema = dataSchema
           .MustNotBeNullOrWhiteSpace()
           .MustBeUri(UriKind.RelativeOrAbsolute, nameof(dataSchema));
        return this;
    }
}
