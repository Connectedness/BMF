using System;
using BrilliantMessaging.Core.Messaging.Outbound;
using BrilliantMessaging.GuardClauses;
using Microsoft.Extensions.DependencyInjection;

namespace BrilliantMessaging.Core.Messaging;

/// <summary>
/// The fluent entry point for configuring BrilliantMessaging after <see cref="BrilliantMessagingServiceCollectionExtensions.AddBrilliantMessaging" />.
/// It
/// exposes the service collection, the message-contract registry builder, and the topology catalog so that the
/// core and the transport packages can layer their registrations onto a single shared configuration.
/// </summary>
/// <remarks>
/// The builder is returned by <see cref="BrilliantMessagingServiceCollectionExtensions.AddBrilliantMessaging" /> and threaded through the
/// transport-specific extension methods (for example the RabbitMQ topology configuration). Calling
/// <see cref="BrilliantMessagingServiceCollectionExtensions.AddBrilliantMessaging" /> more than once returns a builder over the same underlying registry
/// and catalog, so
/// configuration accumulates rather than being replaced.
/// </remarks>
public sealed class BrilliantMessagingBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrilliantMessagingBuilder" /> class.
    /// </summary>
    /// <param name="services">The service collection BrilliantMessaging registers its services into.</param>
    /// <param name="messageContracts">The shared message-contract registry builder.</param>
    /// <param name="topologies">The shared catalog of topology registrations.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null" />.</exception>
    public BrilliantMessagingBuilder(
        IServiceCollection services,
        MessageContractRegistryBuilder messageContracts,
        TopologyRegistrationCatalog topologies
    )
    {
        Services = services.MustNotBeNull();
        MessageContracts = messageContracts.MustNotBeNull();
        Topologies = topologies.MustNotBeNull();
    }

    /// <summary>
    /// Gets the service collection BrilliantMessaging registers its services into.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the shared message-contract registry builder used to map message types to their CloudEvents discriminators.
    /// </summary>
    public MessageContractRegistryBuilder MessageContracts { get; }

    /// <summary>
    /// Gets the shared catalog of topology registrations that transports contribute their topologies to.
    /// </summary>
    public TopologyRegistrationCatalog Topologies { get; }

    /// <summary>
    /// Maps the message contracts (type-to-discriminator mappings) used by the framework.
    /// </summary>
    /// <param name="configure">A callback that adds mappings to the shared <see cref="MessageContracts" /> builder.</param>
    /// <returns>The same <see cref="BrilliantMessagingBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure" /> is <see langword="null" />.</exception>
    public BrilliantMessagingBuilder MapMessageContracts(Action<MessageContractRegistryBuilder> configure)
    {
        configure.MustNotBeNull();
        configure(MessageContracts);
        return this;
    }

    /// <summary>
    /// Configures the <see cref="CloudEventsOptions" /> that supply the framework-wide CloudEvents defaults (most
    /// importantly the <see cref="CloudEventsOptions.Source" />).
    /// </summary>
    /// <param name="configure">A callback that configures the options.</param>
    /// <returns>The same <see cref="BrilliantMessagingBuilder" /> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure" /> is <see langword="null" />.</exception>
    public BrilliantMessagingBuilder UseCloudEvents(Action<CloudEventsOptions> configure)
    {
        configure.MustNotBeNull();
        Services.Configure(configure);
        return this;
    }
}
