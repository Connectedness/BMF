using System;
using System.Collections.Generic;
using BrilliantMessaging.Core.Messaging;
using BrilliantMessaging.Core.Messaging.Inbound;
using BrilliantMessaging.GuardClauses;
using BrilliantMessaging.Transport.Nats.Inbound;
using BrilliantMessaging.Transport.Nats.Outbound;
using NATS.Client.Core;

namespace BrilliantMessaging.Transport.Nats;

/// <summary>
/// Configures a NATS topology. This builder declares JetStream streams and durable consumers, and maps outbound
/// messages to explicit NATS subjects. Core NATS pub/sub is out of scope for this implementation.
/// </summary>
public sealed class NatsTopologyBuilder
    : INatsOutboundTopologyBuilder, INatsInboundTopologyBuilder, IBuildable<NatsTopologyConfiguration>
{
    private readonly List<NatsInboundConsumerDefinition> _consumers = [];
    private readonly List<NatsStreamDefinition> _streams = [];
    private readonly List<NatsOutboundTargetDefinition> _targets = [];

    private bool _ackProgressEnabled = true;
    private Action<MessagePipelineBuilder>? _configurePipeline;

    private Func<IServiceProvider, NatsOpts>? _createOptions = _ => new NatsOpts
    {
        Url = NatsTopologyBuilderDefaults.DefaultServerUrl
    };

    private Type _deserializationMiddlewareType = typeof(MessageDeserializationMiddleware);
    private MessageContractRegistryBuilder? _messageContracts;
    private NatsTopologyProvisioningMode _provisioningMode = NatsTopologyProvisioningMode.CreateOrUpdate;
    private TimeSpan _shutdownTimeout = NatsTopologyBuilderDefaults.DefaultShutdownTimeout;

    /// <inheritdoc />
    NatsTopologyConfiguration IBuildable<NatsTopologyConfiguration>.Build()
    {
        return new NatsTopologyConfiguration(
            _createOptions,
            _streams.AsReadOnly(),
            _targets.AsReadOnly(),
            _consumers.AsReadOnly(),
            _deserializationMiddlewareType,
            _configurePipeline,
            _shutdownTimeout,
            _provisioningMode,
            _ackProgressEnabled,
            (MessageContractRegistry?) ((IBuildable<IMessageContractRegistry>?) _messageContracts)?.Build()
        );
    }

    INatsInboundTopologyBuilder INatsTopologyBuilder<INatsInboundTopologyBuilder>.UseServer(string serverUrl) =>
        UseServer(serverUrl);

    INatsInboundTopologyBuilder INatsTopologyBuilder<INatsInboundTopologyBuilder>.UseOptions(NatsOpts options) =>
        UseOptions(options);

    INatsInboundTopologyBuilder INatsTopologyBuilder<INatsInboundTopologyBuilder>.UseOptions(
        Func<IServiceProvider, NatsOpts> createOptions
    ) => UseOptions(createOptions);

    INatsInboundTopologyBuilder INatsTopologyBuilder<INatsInboundTopologyBuilder>.MapMessageContracts(
        Action<MessageContractRegistryBuilder> configure
    ) => MapMessageContracts(configure);

    INatsInboundTopologyBuilder INatsTopologyBuilder<INatsInboundTopologyBuilder>.Stream(
        string name,
        Action<NatsStreamBuilder> configure
    ) => Stream(name, configure);

    INatsInboundTopologyBuilder INatsTopologyBuilder<INatsInboundTopologyBuilder>.Provisioning(
        NatsTopologyProvisioningMode mode
    ) => Provisioning(mode);

    INatsInboundTopologyBuilder INatsInboundTopologyBuilder.Consume(
        string streamName,
        string durableName,
        Action<NatsInboundConsumerBuilder> configure
    ) => Consume(streamName, durableName, configure);

    INatsInboundTopologyBuilder INatsInboundTopologyBuilder.ConfigureInboundPipeline(
        Action<MessagePipelineBuilder> configure
    ) => ConfigureInboundPipeline(configure);

    INatsInboundTopologyBuilder INatsInboundTopologyBuilder.UseDeserializationMiddleware<TMiddleware>() =>
        UseDeserializationMiddleware<TMiddleware>();

    INatsInboundTopologyBuilder INatsInboundTopologyBuilder.WithShutdownTimeout(TimeSpan shutdownTimeout) =>
        WithShutdownTimeout(shutdownTimeout);

    INatsInboundTopologyBuilder INatsInboundTopologyBuilder.AckProgress(bool enabled) => AckProgress(enabled);

    INatsOutboundTopologyBuilder INatsTopologyBuilder<INatsOutboundTopologyBuilder>.UseServer(string serverUrl) =>
        UseServer(serverUrl);

    INatsOutboundTopologyBuilder INatsTopologyBuilder<INatsOutboundTopologyBuilder>.UseOptions(NatsOpts options) =>
        UseOptions(options);

    INatsOutboundTopologyBuilder INatsTopologyBuilder<INatsOutboundTopologyBuilder>.UseOptions(
        Func<IServiceProvider, NatsOpts> createOptions
    ) => UseOptions(createOptions);

    INatsOutboundTopologyBuilder INatsTopologyBuilder<INatsOutboundTopologyBuilder>.MapMessageContracts(
        Action<MessageContractRegistryBuilder> configure
    ) => MapMessageContracts(configure);

    INatsOutboundTopologyBuilder INatsTopologyBuilder<INatsOutboundTopologyBuilder>.Stream(
        string name,
        Action<NatsStreamBuilder> configure
    ) => Stream(name, configure);

    INatsOutboundTopologyBuilder INatsTopologyBuilder<INatsOutboundTopologyBuilder>.Provisioning(
        NatsTopologyProvisioningMode mode
    ) => Provisioning(mode);

    INatsOutboundTopologyBuilder INatsOutboundTopologyBuilder.Publish<TMessage>(
        Action<NatsOutboundTargetBuilder<TMessage>> configure
    ) => Publish(configure);

    INatsOutboundTopologyBuilder INatsOutboundTopologyBuilder.PublishNamed<TMessage>(
        string targetName,
        Action<NatsOutboundTargetBuilder<TMessage>> configure
    ) => PublishNamed(targetName, configure);

    /// <summary>
    /// Configures the NATS server URI. Credentials and advanced reconnect behaviour can be configured with
    /// <see cref="UseOptions(NatsOpts)" /> or <see cref="UseOptions(Func{IServiceProvider,NatsOpts})" />.
    /// </summary>
    public NatsTopologyBuilder UseServer(string serverUrl)
    {
        serverUrl.MustNotBeNullOrWhiteSpace();

        var capturedUrl = serverUrl;
        _createOptions = _ => new NatsOpts { Url = capturedUrl };
        return this;
    }

    /// <summary>
    /// Configures concrete NATS client options.
    /// </summary>
    public NatsTopologyBuilder UseOptions(NatsOpts options)
    {
        options.MustNotBeNull();

        var capturedOptions = options;
        _createOptions = _ => capturedOptions;
        return this;
    }

    /// <summary>
    /// Configures NATS client options from the application service provider.
    /// </summary>
    public NatsTopologyBuilder UseOptions(Func<IServiceProvider, NatsOpts> createOptions)
    {
        _createOptions = createOptions.MustNotBeNull();
        return this;
    }

    /// <summary>
    /// Configures topology-local message contracts used by this NATS topology.
    /// </summary>
    public NatsTopologyBuilder MapMessageContracts(Action<MessageContractRegistryBuilder> configure)
    {
        configure.MustNotBeNull();

        _messageContracts ??= new MessageContractRegistryBuilder();
        configure(_messageContracts);
        return this;
    }

    /// <summary>
    /// Declares a JetStream stream and its subject patterns.
    /// </summary>
    public NatsTopologyBuilder Stream(string name, Action<NatsStreamBuilder> configure)
    {
        configure.MustNotBeNull();

        NatsStreamBuilder builder = new (name);
        configure(builder);
        _streams.Add(((IBuildable<NatsStreamDefinition>) builder).Build());
        return this;
    }

    /// <summary>
    /// Selects whether the framework creates/updates JetStream resources or only asserts externally managed ones.
    /// </summary>
    public NatsTopologyBuilder Provisioning(NatsTopologyProvisioningMode mode)
    {
        mode.MustBeValidEnumValue();

        _provisioningMode = mode;
        return this;
    }

    /// <summary>
    /// Maps a message type to an explicit NATS subject.
    /// </summary>
    public NatsTopologyBuilder Publish<TMessage>(Action<NatsOutboundTargetBuilder<TMessage>> configure)
    {
        configure.MustNotBeNull();

        NatsOutboundTargetBuilder<TMessage> targetBuilder = new ();
        configure(targetBuilder);
        _targets.Add(((IBuildable<NatsOutboundTargetDefinition>) targetBuilder).Build());
        return this;
    }

    /// <summary>
    /// Maps a named outbound target for a message type to an explicit NATS subject.
    /// </summary>
    public NatsTopologyBuilder PublishNamed<TMessage>(
        string targetName,
        Action<NatsOutboundTargetBuilder<TMessage>> configure
    )
    {
        targetName.MustNotBeNullOrWhiteSpace();

        configure.MustNotBeNull();

        NatsOutboundTargetBuilder<TMessage> targetBuilder = new (targetName);
        configure(targetBuilder);
        _targets.Add(((IBuildable<NatsOutboundTargetDefinition>) targetBuilder).Build());
        return this;
    }

    /// <summary>
    /// Binds a pull-based durable JetStream consumer to a declared stream.
    /// </summary>
    public NatsTopologyBuilder Consume(
        string streamName,
        string durableName,
        Action<NatsInboundConsumerBuilder> configure
    )
    {
        configure.MustNotBeNull();

        NatsInboundConsumerBuilder builder = new (streamName, durableName);
        configure(builder);
        _consumers.Add(((IBuildable<NatsInboundConsumerDefinition>) builder).Build());
        return this;
    }

    /// <summary>
    /// Adds custom middleware to the default inbound pipeline.
    /// </summary>
    public NatsTopologyBuilder ConfigureInboundPipeline(Action<MessagePipelineBuilder> configure)
    {
        _configurePipeline = configure.MustNotBeNull();
        return this;
    }

    /// <summary>
    /// Replaces the default message deserialization middleware.
    /// </summary>
    public NatsTopologyBuilder UseDeserializationMiddleware<TMiddleware>()
        where TMiddleware : class, IMessageMiddleware
    {
        _deserializationMiddlewareType = typeof(TMiddleware);
        return this;
    }

    /// <summary>
    /// Sets the inbound runtime shutdown timeout.
    /// </summary>
    public NatsTopologyBuilder WithShutdownTimeout(TimeSpan shutdownTimeout)
    {
        shutdownTimeout.MustBePositiveOrInfinite();

        _shutdownTimeout = shutdownTimeout;
        return this;
    }

    /// <summary>
    /// Enables or disables periodic JetStream AckProgress heartbeats for in-flight messages.
    /// </summary>
    public NatsTopologyBuilder AckProgress(bool enabled = true)
    {
        _ackProgressEnabled = enabled;
        return this;
    }
}
