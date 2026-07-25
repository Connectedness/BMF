using System;
using System.Collections.Generic;
using BrilliantMessaging.Core.Messaging;
using BrilliantMessaging.Core.Messaging.Inbound;
using BrilliantMessaging.GuardClauses;
using BrilliantMessaging.Transport.RabbitMq.Inbound;
using BrilliantMessaging.Transport.RabbitMq.Outbound;
using RabbitMQ.Client;

namespace BrilliantMessaging.Transport.RabbitMq;

/// <summary>
/// Configures a single RabbitMQ topology. The builder exposes the shared broker-resource surface
/// (<see cref="UseConnectionFactory(ConnectionFactory)" />, <see cref="Exchange" />, <see cref="Queue" />,
/// <see cref="QueueBinding" />, <see cref="ExchangeBinding" />, <see cref="MapMessageContracts" />), outbound
/// publishing configuration (<see cref="Publish{TMessage}" />,
/// <see cref="PublishNamed{TMessage}" />, the outbound <see cref="ChannelGroup(string,int,RabbitMqPublisherConfirmMode?,TimeSpan?)" />
/// overload, and publisher-confirm defaults), and inbound consumer configuration
/// (<see cref="Consume" />, the inbound <see cref="ChannelGroup(string,int,ushort,ushort)" /> overload,
/// <see cref="ConfigureInboundPipeline" />, <see cref="UseDeserializationMiddleware{TMiddleware}" />, and
/// <see cref="WithShutdownTimeout" />). The full surface is available through
/// <see cref="RabbitMqTransportModule.AddRabbitMqTopology(BrilliantMessagingBuilder, Action{RabbitMqTopologyBuilder})" />;
/// <see cref="RabbitMqTransportModule.AddRabbitMqOutboundTopology(BrilliantMessagingBuilder, Action{IRabbitMqOutboundTopologyBuilder})" />
/// and <see cref="RabbitMqTransportModule.AddRabbitMqInboundTopology(BrilliantMessagingBuilder, Action{IRabbitMqInboundTopologyBuilder})" />
/// hand out this builder through the direction-specific interfaces to constrain the configuration surface
/// at compile time.
/// </summary>
public sealed class RabbitMqTopologyBuilder
    : IRabbitMqOutboundTopologyBuilder, IRabbitMqInboundTopologyBuilder, IBuildable<RabbitMqTopologyConfiguration>
{
    private readonly List<RabbitMqBindingDefinition> _bindingDefinitions = [];
    private readonly List<RabbitMqInboundConsumerDefinition> _consumers = [];
    private readonly List<RabbitMqExchangeDefinition> _exchangeDefinitions = [];

    private readonly List<RabbitMqInboundChannelGroupDefinition> _inboundChannelGroupDefinitions = [];
    private readonly List<RabbitMqOutboundChannelGroupDefinition> _outboundChannelGroupDefinitions = [];
    private readonly List<RabbitMqQueueDefinition> _queueDefinitions = [];
    private readonly List<RabbitMqOutboundTargetDefinition> _targets = [];

    private Action<MessagePipelineBuilder>? _configurePipeline;
    private Func<IServiceProvider, ConnectionFactory>? _createConnectionFactory;
    private RabbitMqPublisherConfirmMode _defaultPublisherConfirmMode = RabbitMqPublisherConfirmDefaults.Mode;
    private TimeSpan _defaultPublisherConfirmTimeout = RabbitMqPublisherConfirmDefaults.Timeout;
    private Type _deserializationMiddlewareType = typeof(MessageDeserializationMiddleware);
    private MessageContractRegistryBuilder? _messageContracts;
    private TimeSpan _shutdownTimeout = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    RabbitMqTopologyConfiguration IBuildable<RabbitMqTopologyConfiguration>.Build()
    {
        return new RabbitMqTopologyConfiguration(
            _createConnectionFactory,
            _exchangeDefinitions.AsReadOnly(),
            _queueDefinitions.AsReadOnly(),
            _bindingDefinitions.AsReadOnly(),
            _outboundChannelGroupDefinitions.AsReadOnly(),
            _targets.AsReadOnly(),
            _inboundChannelGroupDefinitions.AsReadOnly(),
            _consumers.AsReadOnly(),
            _deserializationMiddlewareType,
            _configurePipeline,
            _shutdownTimeout,
            _defaultPublisherConfirmMode,
            _defaultPublisherConfirmTimeout,
            (MessageContractRegistry?) ((IBuildable<IMessageContractRegistry>?) _messageContracts)?.Build()
        );
    }

    IRabbitMqInboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqInboundTopologyBuilder>.UseConnectionFactory(
        ConnectionFactory connectionFactory
    ) => UseConnectionFactory(connectionFactory);

    IRabbitMqInboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqInboundTopologyBuilder>.UseConnectionFactory(
        Func<IServiceProvider, ConnectionFactory> createConnectionFactory
    ) => UseConnectionFactory(createConnectionFactory);

    IRabbitMqInboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqInboundTopologyBuilder>.Exchange(
        string name,
        string type,
        Action<RabbitMqExchangeBuilder>? configure
    ) => Exchange(name, type, configure);

    IRabbitMqInboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqInboundTopologyBuilder>.Queue(
        string name,
        Action<RabbitMqQueueBuilder>? configure
    ) => Queue(name, configure);

    IRabbitMqInboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqInboundTopologyBuilder>.QueueBinding(
        string exchangeName,
        string queueName,
        string routingKey,
        Action<RabbitMqQueueBindingBuilder>? configure
    ) => QueueBinding(exchangeName, queueName, routingKey, configure);

    IRabbitMqInboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqInboundTopologyBuilder>.ExchangeBinding(
        string sourceExchangeName,
        string destinationExchangeName,
        string routingKey,
        Action<RabbitMqExchangeBindingBuilder>? configure
    ) => ExchangeBinding(sourceExchangeName, destinationExchangeName, routingKey, configure);

    IRabbitMqInboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqInboundTopologyBuilder>.MapMessageContracts(
        Action<MessageContractRegistryBuilder> configure
    ) => MapMessageContracts(configure);

    IRabbitMqInboundTopologyBuilder IRabbitMqInboundTopologyBuilder.ChannelGroup(
        string name,
        int maximumChannelCount,
        ushort prefetchCount,
        ushort consumerDispatchConcurrency
    ) => ChannelGroup(name, maximumChannelCount, prefetchCount, consumerDispatchConcurrency);

    IRabbitMqInboundTopologyBuilder IRabbitMqInboundTopologyBuilder.Consume(
        string queueName,
        Action<RabbitMqInboundConsumerBuilder> configure
    ) => Consume(queueName, configure);

    IRabbitMqInboundTopologyBuilder IRabbitMqInboundTopologyBuilder.ConfigureInboundPipeline(
        Action<MessagePipelineBuilder> configure
    ) => ConfigureInboundPipeline(configure);

    IRabbitMqInboundTopologyBuilder IRabbitMqInboundTopologyBuilder.UseDeserializationMiddleware<TMiddleware>() =>
        UseDeserializationMiddleware<TMiddleware>();

    IRabbitMqInboundTopologyBuilder IRabbitMqInboundTopologyBuilder.WithShutdownTimeout(
        TimeSpan shutdownTimeout
    ) => WithShutdownTimeout(shutdownTimeout);

    // Explicit bridges for IRabbitMqOutboundTopologyBuilder and IRabbitMqInboundTopologyBuilder. C# does not
    // allow covariant return types on interface implementations, so the public members above (returning
    // RabbitMqTopologyBuilder) cannot satisfy the interfaces implicitly.

    IRabbitMqOutboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqOutboundTopologyBuilder>.UseConnectionFactory(
        ConnectionFactory connectionFactory
    ) => UseConnectionFactory(connectionFactory);

    IRabbitMqOutboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqOutboundTopologyBuilder>.UseConnectionFactory(
        Func<IServiceProvider, ConnectionFactory> createConnectionFactory
    ) => UseConnectionFactory(createConnectionFactory);

    IRabbitMqOutboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqOutboundTopologyBuilder>.Exchange(
        string name,
        string type,
        Action<RabbitMqExchangeBuilder>? configure
    ) => Exchange(name, type, configure);

    IRabbitMqOutboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqOutboundTopologyBuilder>.Queue(
        string name,
        Action<RabbitMqQueueBuilder>? configure
    ) => Queue(name, configure);

    IRabbitMqOutboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqOutboundTopologyBuilder>.QueueBinding(
        string exchangeName,
        string queueName,
        string routingKey,
        Action<RabbitMqQueueBindingBuilder>? configure
    ) => QueueBinding(exchangeName, queueName, routingKey, configure);

    IRabbitMqOutboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqOutboundTopologyBuilder>.ExchangeBinding(
        string sourceExchangeName,
        string destinationExchangeName,
        string routingKey,
        Action<RabbitMqExchangeBindingBuilder>? configure
    ) => ExchangeBinding(sourceExchangeName, destinationExchangeName, routingKey, configure);

    IRabbitMqOutboundTopologyBuilder IRabbitMqTopologyBuilder<IRabbitMqOutboundTopologyBuilder>.MapMessageContracts(
        Action<MessageContractRegistryBuilder> configure
    ) => MapMessageContracts(configure);

    IRabbitMqOutboundTopologyBuilder IRabbitMqOutboundTopologyBuilder.Publish<TMessage>(
        Action<RabbitMqOutboundTargetBuilder<TMessage>> configure
    ) => Publish(configure);

    IRabbitMqOutboundTopologyBuilder IRabbitMqOutboundTopologyBuilder.PublishNamed<TMessage>(
        string targetName,
        Action<RabbitMqOutboundTargetBuilder<TMessage>> configure
    ) => PublishNamed(targetName, configure);

    IRabbitMqOutboundTopologyBuilder IRabbitMqOutboundTopologyBuilder.ChannelGroup(
        string name,
        int maximumChannelCount,
        RabbitMqPublisherConfirmMode? publisherConfirmMode,
        TimeSpan? publisherConfirmTimeout
    ) => ChannelGroup(name, maximumChannelCount, publisherConfirmMode, publisherConfirmTimeout);

    IRabbitMqOutboundTopologyBuilder IRabbitMqOutboundTopologyBuilder.WithDefaultPublisherConfirmMode(
        RabbitMqPublisherConfirmMode publisherConfirmMode
    ) => WithDefaultPublisherConfirmMode(publisherConfirmMode);

    IRabbitMqOutboundTopologyBuilder IRabbitMqOutboundTopologyBuilder.WithDefaultPublisherConfirmTimeout(
        TimeSpan publisherConfirmTimeout
    ) => WithDefaultPublisherConfirmTimeout(publisherConfirmTimeout);

    /// <inheritdoc cref="IRabbitMqTopologyBuilder{TSelf}.UseConnectionFactory(ConnectionFactory)" />
    public RabbitMqTopologyBuilder UseConnectionFactory(ConnectionFactory connectionFactory)
    {
        connectionFactory.MustNotBeNull();

        var capturedFactory = connectionFactory;
        _createConnectionFactory = _ => capturedFactory;
        return this;
    }

    /// <inheritdoc cref="UseConnectionFactory(ConnectionFactory)" />
    public RabbitMqTopologyBuilder UseConnectionFactory(
        Func<IServiceProvider, ConnectionFactory> createConnectionFactory
    )
    {
        _createConnectionFactory = createConnectionFactory.MustNotBeNull();
        return this;
    }

    /// <inheritdoc cref="IRabbitMqTopologyBuilder{TSelf}.Exchange" />
    public RabbitMqTopologyBuilder Exchange(
        string name,
        string type,
        Action<RabbitMqExchangeBuilder>? configure = null
    )
    {
        RabbitMqExchangeBuilder builder = new (name, type);
        configure?.Invoke(builder);
        _exchangeDefinitions.Add(((IBuildable<RabbitMqExchangeDefinition>) builder).Build());
        return this;
    }

    /// <inheritdoc cref="IRabbitMqTopologyBuilder{TSelf}.Queue" />
    public RabbitMqTopologyBuilder Queue(string name, Action<RabbitMqQueueBuilder>? configure = null)
    {
        RabbitMqQueueBuilder builder = new (name);
        configure?.Invoke(builder);
        _queueDefinitions.Add(((IBuildable<RabbitMqQueueDefinition>) builder).Build());
        return this;
    }

    /// <inheritdoc cref="IRabbitMqTopologyBuilder{TSelf}.QueueBinding" />
    public RabbitMqTopologyBuilder QueueBinding(
        string exchangeName,
        string queueName,
        string routingKey = "",
        Action<RabbitMqQueueBindingBuilder>? configure = null
    )
    {
        RabbitMqQueueBindingBuilder builder = new (exchangeName, queueName, routingKey);
        configure?.Invoke(builder);
        _bindingDefinitions.Add(((IBuildable<RabbitMqQueueBindingDefinition>) builder).Build());
        return this;
    }

    /// <inheritdoc cref="IRabbitMqTopologyBuilder{TSelf}.ExchangeBinding" />
    public RabbitMqTopologyBuilder ExchangeBinding(
        string sourceExchangeName,
        string destinationExchangeName,
        string routingKey = "",
        Action<RabbitMqExchangeBindingBuilder>? configure = null
    )
    {
        RabbitMqExchangeBindingBuilder builder = new (sourceExchangeName, destinationExchangeName, routingKey);
        configure?.Invoke(builder);
        _bindingDefinitions.Add(((IBuildable<RabbitMqExchangeBindingDefinition>) builder).Build());
        return this;
    }

    /// <inheritdoc cref="IRabbitMqTopologyBuilder{TSelf}.MapMessageContracts" />
    public RabbitMqTopologyBuilder MapMessageContracts(Action<MessageContractRegistryBuilder> configure)
    {
        configure.MustNotBeNull();

        _messageContracts ??= new MessageContractRegistryBuilder();
        configure(_messageContracts);
        return this;
    }

    /// <inheritdoc cref="IRabbitMqOutboundTopologyBuilder.WithDefaultPublisherConfirmMode" />
    public RabbitMqTopologyBuilder WithDefaultPublisherConfirmMode(
        RabbitMqPublisherConfirmMode publisherConfirmMode
    )
    {
        ValidatePublisherConfirmMode(publisherConfirmMode, nameof(publisherConfirmMode));
        _defaultPublisherConfirmMode = publisherConfirmMode;
        return this;
    }

    /// <inheritdoc cref="IRabbitMqOutboundTopologyBuilder.WithDefaultPublisherConfirmTimeout" />
    public RabbitMqTopologyBuilder WithDefaultPublisherConfirmTimeout(TimeSpan publisherConfirmTimeout)
    {
        ValidatePublisherConfirmTimeout(publisherConfirmTimeout, nameof(publisherConfirmTimeout));
        _defaultPublisherConfirmTimeout = publisherConfirmTimeout;
        return this;
    }

    /// <inheritdoc cref="IRabbitMqOutboundTopologyBuilder.ChannelGroup" />
    public RabbitMqTopologyBuilder ChannelGroup(
        string name,
        int maximumChannelCount,
        RabbitMqPublisherConfirmMode? publisherConfirmMode = null,
        TimeSpan? publisherConfirmTimeout = null
    )
    {
        maximumChannelCount.MustBePositive();

        if (publisherConfirmMode is not null)
        {
            ValidatePublisherConfirmMode(publisherConfirmMode.Value, nameof(publisherConfirmMode));
        }

        if (publisherConfirmTimeout is not null)
        {
            ValidatePublisherConfirmTimeout(publisherConfirmTimeout.Value, nameof(publisherConfirmTimeout));
        }

        var channelGroupName = name.MustNotBeNullOrWhiteSpace();

        channelGroupName.MustNotStartWith(
            RabbitMqOutboundChannelGroupDefinition.ReservedImplicitNamePrefix,
            StringComparison.Ordinal,
            nameof(name),
            $"Channel group names beginning with '{RabbitMqOutboundChannelGroupDefinition.ReservedImplicitNamePrefix}' are reserved."
        );

        _outboundChannelGroupDefinitions.Add(
            new RabbitMqOutboundChannelGroupDefinition(
                channelGroupName,
                maximumChannelCount,
                publisherConfirmMode,
                publisherConfirmTimeout
            )
        );
        return this;
    }

    /// <inheritdoc cref="IRabbitMqInboundTopologyBuilder.ChannelGroup" />
    public RabbitMqTopologyBuilder ChannelGroup(
        string name,
        int maximumChannelCount,
        ushort prefetchCount,
        ushort consumerDispatchConcurrency
    )
    {
        maximumChannelCount.MustBePositive();
        prefetchCount.MustBePositive();
        consumerDispatchConcurrency.MustBePositive();
        var channelGroupName = name.MustNotBeNullOrWhiteSpace();
        channelGroupName.MustNotStartWith(
            RabbitMqInboundChannelGroupDefinition.ReservedImplicitNamePrefix,
            StringComparison.Ordinal,
            nameof(name),
            $"Channel group names beginning with '{RabbitMqInboundChannelGroupDefinition.ReservedImplicitNamePrefix}' are reserved."
        );

        _inboundChannelGroupDefinitions.Add(
            new RabbitMqInboundChannelGroupDefinition(
                channelGroupName,
                maximumChannelCount,
                prefetchCount,
                consumerDispatchConcurrency
            )
        );
        return this;
    }

    /// <inheritdoc cref="IRabbitMqOutboundTopologyBuilder.Publish{TMessage}" />
    public RabbitMqTopologyBuilder Publish<TMessage>(
        Action<RabbitMqOutboundTargetBuilder<TMessage>> configure
    )
    {
        return PublishCore(null, configure);
    }

    /// <inheritdoc cref="IRabbitMqOutboundTopologyBuilder.PublishNamed{TMessage}" />
    public RabbitMqTopologyBuilder PublishNamed<TMessage>(
        string targetName,
        Action<RabbitMqOutboundTargetBuilder<TMessage>> configure
    )
    {
        return PublishCore(targetName.MustNotBeNullOrWhiteSpace(), configure);
    }

    /// <inheritdoc cref="IRabbitMqInboundTopologyBuilder.Consume" />
    public RabbitMqTopologyBuilder Consume(
        string queueName,
        Action<RabbitMqInboundConsumerBuilder> configure
    )
    {
        configure.MustNotBeNull();

        RabbitMqInboundConsumerBuilder builder = new (queueName);
        configure(builder);
        _consumers.Add(((IBuildable<RabbitMqInboundConsumerDefinition>) builder).Build());
        return this;
    }

    /// <inheritdoc cref="IRabbitMqInboundTopologyBuilder.ConfigureInboundPipeline" />
    public RabbitMqTopologyBuilder ConfigureInboundPipeline(Action<MessagePipelineBuilder> configure)
    {
        configure.MustNotBeNull();

        _configurePipeline += configure;
        return this;
    }

    /// <inheritdoc cref="IRabbitMqInboundTopologyBuilder.UseDeserializationMiddleware{TMiddleware}" />
    public RabbitMqTopologyBuilder UseDeserializationMiddleware<TMiddleware>()
        where TMiddleware : class, IMessageMiddleware
    {
        _deserializationMiddlewareType = typeof(TMiddleware);
        return this;
    }

    /// <inheritdoc cref="IRabbitMqInboundTopologyBuilder.WithShutdownTimeout" />
    public RabbitMqTopologyBuilder WithShutdownTimeout(TimeSpan shutdownTimeout)
    {
        shutdownTimeout.MustBePositive();

        _shutdownTimeout = shutdownTimeout;
        return this;
    }

    private RabbitMqTopologyBuilder PublishCore<TMessage>(
        string? targetName,
        Action<RabbitMqOutboundTargetBuilder<TMessage>> configure
    )
    {
        configure.MustNotBeNull();

        RabbitMqOutboundTargetBuilder<TMessage> builder = new (targetName);
        configure(builder);
        _targets.Add(((IBuildable<RabbitMqOutboundTargetDefinition>) builder).Build());
        return this;
    }

    private static void ValidatePublisherConfirmMode(
        RabbitMqPublisherConfirmMode publisherConfirmMode,
        string parameterName
    )
    {
        publisherConfirmMode.MustBeValidEnumValue(parameterName);
    }

    private static void ValidatePublisherConfirmTimeout(TimeSpan publisherConfirmTimeout, string parameterName)
    {
        publisherConfirmTimeout.MustBeIn(
            new Range<TimeSpan>(
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(uint.MaxValue - 1d),
                isFromInclusive: false
            ),
            parameterName
        );
    }
}
