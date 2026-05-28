using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Usf.Core.Messaging;
using Usf.Core.Messaging.Errors;
using Usf.Core.Messaging.Serialization;
using Usf.Transport.RabbitMq.Configuration;
using Usf.Transport.RabbitMq.Tests.TestSupport;
using Xunit;

namespace Usf.Transport.RabbitMq.Tests.Unit;

public sealed class RabbitMqChannelPoolingTests
{
    [Fact]
    public void RabbitMqMessagePublishingBuilder_UsesExpectedChannelPoolingDefaults()
    {
        var builder = new RabbitMqMessagePublishingBuilder();

        builder.UseConnectionFactory(static _ => new ConnectionFactory());

        var configuration = builder.Build();

        configuration.ChannelPoolingMode.Should().Be(RabbitMqChannelPoolingMode.PerTarget);
        configuration.MaxChannelsPerTarget.Should().Be(1);
        configuration.SharedChannelPoolSize.Should().Be(8);
    }

    [Fact]
    public async Task RabbitMqChannelPool_ReusesHealthyChannelsSequentially()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createdChannels = new List<TestRabbitMqChannel>();
        await using var pool = new DefaultRabbitMqChannelPool(
            1,
            _ =>
            {
                var channel = new TestRabbitMqChannel();
                createdChannels.Add(channel);
                return Task.FromResult(channel.Object);
            }
        );

        IChannel firstChannel;
        await using (var lease = await pool.AcquireAsync(cancellationToken))
        {
            firstChannel = lease.Channel;
        }

        await using var secondLease = await pool.AcquireAsync(cancellationToken);

        secondLease.Channel.Should().BeSameAs(firstChannel);
        createdChannels.Should().HaveCount(1);
    }

    [Fact]
    public async Task RabbitMqChannelPool_WaitsForReturnedChannelsWhenBoundIsReached()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var channel = new TestRabbitMqChannel();
        await using var pool = new DefaultRabbitMqChannelPool(1, _ => Task.FromResult(channel.Object));

        var firstLease = await pool.AcquireAsync(cancellationToken);
        var waitingAcquire = pool.AcquireAsync(cancellationToken).AsTask();

        waitingAcquire.IsCompleted.Should().BeFalse();

        await firstLease.DisposeAsync();
        await using var secondLease = await waitingAcquire.WaitAsync(TimeSpan.FromSeconds(2), cancellationToken);

        secondLease.Channel.Should().BeSameAs(channel.Object);
    }

    [Fact]
    public async Task RabbitMqChannelPool_AcquiresDistinctChannelsForConcurrentLeases()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var createdChannels = new List<TestRabbitMqChannel>();
        await using var pool = new DefaultRabbitMqChannelPool(
            2,
            _ =>
            {
                var channel = new TestRabbitMqChannel();
                createdChannels.Add(channel);
                return Task.FromResult(channel.Object);
            }
        );

        var firstLeaseTask = pool.AcquireAsync(cancellationToken).AsTask();
        var secondLeaseTask = pool.AcquireAsync(cancellationToken).AsTask();
        var leases = await Task.WhenAll(firstLeaseTask, secondLeaseTask);

        try
        {
            leases[0].Channel.Should().NotBeSameAs(leases[1].Channel);
            createdChannels.Should().HaveCount(2);
        }
        finally
        {
            await leases[0].DisposeAsync();
            await leases[1].DisposeAsync();
        }
    }

    [Fact]
    public async Task RabbitMqChannelPool_ReplacesChannelsThatFaultWhileLeased()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstChannel = new TestRabbitMqChannel();
        var secondChannel = new TestRabbitMqChannel();
        var channels = new Queue<TestRabbitMqChannel>([firstChannel, secondChannel]);
        await using var pool = new DefaultRabbitMqChannelPool(1, _ => Task.FromResult(channels.Dequeue().Object));

        IChannel leasedChannel;
        var lease = await pool.AcquireAsync(cancellationToken);
        leasedChannel = lease.Channel;
        await firstChannel.ShutdownAsync();
        await lease.DisposeAsync();

        await using var replacementLease = await pool.AcquireAsync(cancellationToken);

        replacementLease.Channel.Should().NotBeSameAs(leasedChannel);
        replacementLease.Channel.Should().BeSameAs(secondChannel.Object);
        firstChannel.DisposeAsyncCallCount.Should().Be(1);
    }

    [Fact]
    public async Task RabbitMqChannelPool_PropagatesCancellationWhileWaitingForReturnedChannel()
    {
        var channel = new TestRabbitMqChannel();
        await using var pool = new DefaultRabbitMqChannelPool(1, _ => Task.FromResult(channel.Object));
        await using var holdingLease = await pool.AcquireAsync(TestContext.Current.CancellationToken);

        using var cancellationTokenSource = new CancellationTokenSource();
        var waitingAcquire = pool.AcquireAsync(cancellationTokenSource.Token).AsTask();

        waitingAcquire.IsCompleted.Should().BeFalse();
        await cancellationTokenSource.CancelAsync();

        Func<Task> action = async () => await waitingAcquire;
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RabbitMqTarget_ReusesChannelWhenPublishFailsButChannelStaysOpen()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var channel = new TestRabbitMqChannel();
        var firstAttempt = true;
        channel.BasicPublishAsyncHandler = () =>
        {
            if (firstAttempt)
            {
                firstAttempt = false;
                throw new InvalidOperationException("Broker rejected message.");
            }

            return default;
        };

        await using var pool = new DefaultRabbitMqChannelPool(1, _ => Task.FromResult(channel.Object));
        var target = new RabbitMqFanoutTarget<ValidationMessageA>(
            "target",
            new Utf8JsonMessageSerializer(),
            pool,
            false,
            "exchange",
            false
        );

        Func<Task> firstPublish = async () =>
            await target.PublishAsync(new ValidationMessageA("first"), cancellationToken);

        await firstPublish.Should().ThrowAsync<InvalidOperationException>();
        await target.PublishAsync(new ValidationMessageA("second"), cancellationToken);

        channel.BasicPublishCallCount.Should().Be(2);
        channel.DisposeAsyncCallCount.Should().Be(0);
    }

    [Fact]
    public async Task RabbitMqTarget_ReleasesLeaseSlotWhenPublishFaultsAndClosesChannel()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstChannel = new TestRabbitMqChannel();
        var secondChannel = new TestRabbitMqChannel();
        var channels = new Queue<TestRabbitMqChannel>([firstChannel, secondChannel]);
        firstChannel.BasicPublishAsyncHandler = async () =>
        {
            await firstChannel.ShutdownAsync().ConfigureAwait(false);
            throw new InvalidOperationException("Publish failed.");
        };

        await using var pool = new DefaultRabbitMqChannelPool(1, _ => Task.FromResult(channels.Dequeue().Object));
        var target = new RabbitMqFanoutTarget<ValidationMessageA>(
            "target",
            new Utf8JsonMessageSerializer(),
            pool,
            false,
            "exchange",
            false
        );

        Func<Task> firstPublish = async () =>
            await target.PublishAsync(new ValidationMessageA("first"), cancellationToken);

        await firstPublish.Should().ThrowAsync<InvalidOperationException>();
        await target.PublishAsync(new ValidationMessageA("second"), cancellationToken);

        firstChannel.DisposeAsyncCallCount.Should().Be(1);
        secondChannel.BasicPublishCallCount.Should().Be(1);
    }

    [Fact]
    public async Task RabbitMqConnectionManager_ThrowsWhenWorstCaseChannelCountExceedsBrokerLimit()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var builder = new RabbitMqMessagePublishingBuilder();
        builder.UseConnectionFactory(static _ => new ConnectionFactory());
        builder.UseMaxChannelsPerTarget(2);
        builder.Exchange("orders", ExchangeType.Fanout);
        builder.Publish<ValidationMessageA>(
            route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
        );
        builder.PublishNamed<ValidationMessageA>(
            "secondary",
            route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
        );

        var configuration = builder.Build();
        var connection = new TestRabbitMqConnection
        {
            ChannelMax = 3
        };
        var connectionManager = new RabbitMqConnectionManager(
            configuration,
            _ => Task.FromResult(connection.Object)
        );

        Func<Task> action = async () => await connectionManager.GetConnectionAsync(cancellationToken);

        var exception = await action.Should().ThrowAsync<MessageTopologyValidationException>();
        exception.Which.ValidationErrors.Should().ContainSingle();
        exception.Which.ValidationErrors[0].Should()
           .Be(
                "RabbitMQ publish topology may open up to 4 channels (PerTarget mode, 2 targets × max 2), but the broker negotiated channel_max=3."
            );
        connection.DisposeAsyncCallCount.Should().Be(1);
    }

    [Fact]
    public async Task RabbitMqConnectionManager_SkipsChannelLimitCheckWhenBrokerAdvertisesUnlimitedChannels()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var builder = new RabbitMqMessagePublishingBuilder();
        builder.UseConnectionFactory(static _ => new ConnectionFactory());
        builder.UseMaxChannelsPerTarget(50);
        builder.Exchange("orders", ExchangeType.Fanout);
        builder.Publish<ValidationMessageA>(
            route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
        );

        var configuration = builder.Build();
        var connection = new TestRabbitMqConnection
        {
            ChannelMax = 0
        };
        var connectionManager = new RabbitMqConnectionManager(
            configuration,
            _ => Task.FromResult(connection.Object)
        );

        var resolved = await connectionManager.GetConnectionAsync(cancellationToken);

        resolved.Should().BeSameAs(connection.Object);
        connection.DisposeAsyncCallCount.Should().Be(0);
    }

    [Fact]
    public void RabbitMqMessageTopologyCompiler_LogsWorstCaseChannelCountAtCompileTime()
    {
        var loggerProvider = new RecordingLoggerProvider();
        using var loggerFactory = new RecordingLoggerFactory(loggerProvider);
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton<Utf8JsonMessageSerializer>();
        services.AddRabbitMqMessagePublishing(
            builder =>
            {
                builder.UseConnectionFactory(static _ => new ConnectionFactory());
                builder.UseChannelPoolingMode(RabbitMqChannelPoolingMode.Shared);
                builder.UseSharedChannelPoolSize(11);
                builder.Exchange("orders", ExchangeType.Fanout);
                builder.Publish<ValidationMessageA>(
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
            }
        );
        using var serviceProvider = services.BuildServiceProvider();

        _ = serviceProvider.GetRequiredService<RabbitMqCompiledTopology>();

        loggerProvider.Entries.Should().Contain(
            entry => entry.LogLevel == LogLevel.Information &&
                     entry.Message ==
                     "RabbitMQ publish topology may open up to 11 channels (Shared mode, shared pool size 11)."
        );
    }

    [Fact]
    public void RabbitMqMessageTopologyCompiler_AssignsSharedPoolToEveryTargetInSharedMode()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Utf8JsonMessageSerializer>();
        services.AddRabbitMqMessagePublishing(
            builder =>
            {
                builder.UseConnectionFactory(static _ => new ConnectionFactory());
                builder.UseChannelPoolingMode(RabbitMqChannelPoolingMode.Shared);
                builder.UseSharedChannelPoolSize(2);
                builder.Exchange("orders", ExchangeType.Fanout);
                builder.Publish<ValidationMessageA>(
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
                builder.PublishNamed<ValidationMessageA>(
                    "secondary",
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
                builder.PublishNamed<ValidationMessageA>(
                    "tertiary",
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
            }
        );
        using var serviceProvider = services.BuildServiceProvider();

        var topology = serviceProvider.GetRequiredService<RabbitMqCompiledTopology>();
        var targets = EnumerateTargets(topology).ToList();
        var pools = targets.Select(GetChannelPool).Distinct().ToList();

        targets.Should().HaveCount(3);
        pools.Should().ContainSingle("all targets must share the same pool in Shared mode");
    }

    [Fact]
    public void RabbitMqMessageTopologyCompiler_GivesEachTargetItsOwnPoolInPerTargetMode()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Utf8JsonMessageSerializer>();
        services.AddRabbitMqMessagePublishing(
            builder =>
            {
                builder.UseConnectionFactory(static _ => new ConnectionFactory());
                builder.Exchange("orders", ExchangeType.Fanout);
                builder.Publish<ValidationMessageA>(
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
                builder.PublishNamed<ValidationMessageA>(
                    "secondary",
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
                builder.PublishNamed<ValidationMessageA>(
                    "tertiary",
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
            }
        );
        using var serviceProvider = services.BuildServiceProvider();

        var topology = serviceProvider.GetRequiredService<RabbitMqCompiledTopology>();
        var targets = EnumerateTargets(topology).ToList();
        var pools = targets.Select(GetChannelPool).ToList();

        targets.Should().HaveCount(3);
        pools.Distinct().Should().HaveCount(3, "each target must own a distinct pool in PerTarget mode");
    }

    [Fact]
    public async Task RabbitMqCompiledTopology_PerTargetModeDisposesEveryOwnedPoolExactlyOnce()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Utf8JsonMessageSerializer>();
        services.AddRabbitMqMessagePublishing(
            builder =>
            {
                builder.UseConnectionFactory(static _ => new ConnectionFactory());
                builder.Exchange("orders", ExchangeType.Fanout);
                builder.Publish<ValidationMessageA>(
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
                builder.PublishNamed<ValidationMessageA>(
                    "secondary",
                    route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
                );
            }
        );
        await using var serviceProvider = services.BuildServiceProvider();

        var topology = serviceProvider.GetRequiredService<RabbitMqCompiledTopology>();
        var pools = EnumerateTargets(topology).Select(GetChannelPool).Cast<DefaultRabbitMqChannelPool>().ToList();

        await topology.DisposeAsync();

        Func<Task> reAcquire = async () =>
            await pools[0].AcquireAsync(TestContext.Current.CancellationToken);

        pools.Should().HaveCount(2);
        await reAcquire.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task RabbitMqCompiledTopology_DisposesTargetsBeforeSharedPool()
    {
        var disposalEvents = new List<string>();
        var sharedPool = new TrackingChannelPool("shared-pool", disposalEvents);
        var firstTarget = new TrackingTarget("target-a", disposalEvents);
        var secondTarget = new TrackingTarget("target-b", disposalEvents);
        var topology = new RabbitMqCompiledTopology(
            new MessageTopology(new Dictionary<Type, Target>(), new Dictionary<string, Target>(StringComparer.Ordinal)),
            Array.Empty<RabbitMqExchangeDefinition>(),
            Array.Empty<RabbitMqQueueDefinition>(),
            Array.Empty<RabbitMqBindingDefinition>(),
            [firstTarget, secondTarget],
            sharedPool
        );

        await topology.DisposeAsync();

        disposalEvents.Should().Equal("target-a", "target-b", "shared-pool");
    }

    [Fact]
    public async Task RabbitMqCompiledTopology_DisposesConnectionManagerAfterChannelPool()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var disposalEvents = new List<string>();
        var connection = new TestRabbitMqConnection(disposalEvents);
        var builder = new RabbitMqMessagePublishingBuilder();
        builder.UseConnectionFactory(static _ => new ConnectionFactory());
        builder.Exchange("orders", ExchangeType.Fanout);
        builder.Publish<ValidationMessageA>(
            route => route.ToFanoutExchange("orders").WithSerializer<Utf8JsonMessageSerializer>()
        );
        var configuration = builder.Build();
        var connectionManager = new RabbitMqConnectionManager(
            configuration,
            _ => Task.FromResult(connection.Object)
        );
        _ = await connectionManager.GetConnectionAsync(cancellationToken);

        var sharedPool = new TrackingChannelPool("shared-pool", disposalEvents);
        var topology = new RabbitMqCompiledTopology(
            new MessageTopology(new Dictionary<Type, Target>(), new Dictionary<string, Target>(StringComparer.Ordinal)),
            Array.Empty<RabbitMqExchangeDefinition>(),
            Array.Empty<RabbitMqQueueDefinition>(),
            Array.Empty<RabbitMqBindingDefinition>(),
            Array.Empty<Target>(),
            sharedPool,
            connectionManager
        );

        await topology.DisposeAsync();

        disposalEvents.Should().Equal("shared-pool", "connection");
    }

    private static IEnumerable<Target> EnumerateTargets(RabbitMqCompiledTopology topology)
    {
        var field = typeof(RabbitMqCompiledTopology).GetField(
            "_targets",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        return (IReadOnlyList<Target>) field!.GetValue(topology)!;
    }

    private static IRabbitMqChannelPool GetChannelPool(Target target)
    {
        var rabbitMqTargetType = target.GetType();
        while (rabbitMqTargetType is not null &&
               (!rabbitMqTargetType.IsGenericType ||
                rabbitMqTargetType.GetGenericTypeDefinition() != typeof(RabbitMqTarget<>)))
        {
            rabbitMqTargetType = rabbitMqTargetType.BaseType;
        }

        var field = rabbitMqTargetType!.GetField("_channelPool", BindingFlags.Instance | BindingFlags.NonPublic);
        return (IRabbitMqChannelPool) field!.GetValue(target)!;
    }
}
