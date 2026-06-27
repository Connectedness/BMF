using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrilliantMessaging.Core.Messaging;
using BrilliantMessaging.Core.Messaging.Inbound;
using BrilliantMessaging.Core.Messaging.Outbound;
using BrilliantMessaging.Transport.RabbitMq.Inbound;
using BrilliantMessaging.Transport.RabbitMq.Tests.TestSupport;
using FluentAssertions;
using RabbitMQ.Client;
using Xunit;

namespace BrilliantMessaging.Transport.RabbitMq.Tests.Unit;

public sealed class RabbitMqTopologyProvisionerDeleteModeTests
{
    [Fact]
    public async Task ProvisionAsync_ThrowsForExchangeDeleteMode()
    {
        RabbitMqExchangeDefinition exchange = new (
            "bad-exchange",
            ExchangeType.Direct,
            RabbitMqDeclareMode.Delete,
            true,
            false,
            new Dictionary<string, object?>()
        );
        var topology = BuildMinimalTopology(exchanges: [exchange]);

        var provisioner = new RabbitMqTopologyProvisioner(topology);
        var act = async () => await provisioner.ProvisionAsync(CancellationToken.None);

        (await act.Should().ThrowAsync<ArgumentOutOfRangeException>())
           .Which.Message.Should().Contain("Exchange deletion is not supported");
    }

    [Fact]
    public async Task ProvisionAsync_ThrowsForExchangeBindingDeleteMode()
    {
        RabbitMqExchangeDefinition sourceExchange = new (
            "source",
            ExchangeType.Direct,
            RabbitMqDeclareMode.Active,
            true,
            false,
            new Dictionary<string, object?>()
        );
        RabbitMqExchangeDefinition destinationExchange = new (
            "destination",
            ExchangeType.Direct,
            RabbitMqDeclareMode.Active,
            true,
            false,
            new Dictionary<string, object?>()
        );
        RabbitMqExchangeBindingDefinition exchangeBinding = new (
            "source",
            "destination",
            "routing",
            RabbitMqBindingMode.Delete,
            new Dictionary<string, object?>()
        );
        var topology = BuildMinimalTopology(
            exchanges: [sourceExchange, destinationExchange],
            bindings: [exchangeBinding]
        );

        var provisioner = new RabbitMqTopologyProvisioner(topology);
        var act = async () => await provisioner.ProvisionAsync(CancellationToken.None);

        (await act.Should().ThrowAsync<ArgumentOutOfRangeException>())
           .Which.Message.Should().Contain("Exchange binding deletion is not supported");
    }

    [Fact]
    public async Task ProvisionAsync_SkipsQueueInSkipMode()
    {
        RabbitMqQueueDefinition queue = new (
            "skip-queue",
            RabbitMqDeclareMode.Skip,
            true,
            false,
            false,
            new Dictionary<string, object?>()
        );
        var topology = BuildMinimalTopology(queues: [queue]);

        var provisioner = new RabbitMqTopologyProvisioner(topology);
        var act = async () => await provisioner.ProvisionAsync(CancellationToken.None);

        await act.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public async Task ProvisionAsync_PassivelyDeclaresQueueInPassiveMode()
    {
        RabbitMqQueueDefinition queue = new (
            "passive-queue",
            RabbitMqDeclareMode.Passive,
            true,
            false,
            false,
            new Dictionary<string, object?>()
        );
        var topology = BuildMinimalTopology(queues: [queue]);

        var provisioner = new RabbitMqTopologyProvisioner(topology);
        var act = async () => await provisioner.ProvisionAsync(CancellationToken.None);

        // The test channel's QueueDeclarePassiveAsync returns the default (null Task or default value),
        // so this should not throw. The dispatch proxy handles it by returning the default value.
        await act.Should().NotThrowAsync<Exception>();
    }

    private static RabbitMqTopology BuildMinimalTopology(
        IReadOnlyList<RabbitMqExchangeDefinition>? exchanges = null,
        IReadOnlyList<RabbitMqQueueDefinition>? queues = null,
        IReadOnlyList<RabbitMqBindingDefinition>? bindings = null
    )
    {
        TestRabbitMqChannel testChannel = new ();
        TestRabbitMqConnection testConnection = new ();
        testConnection.EnqueueChannel(testChannel.Object);
        RabbitMqConnectionProvider connectionProvider = new (
            _ => Task.FromResult(testConnection.Object)
        );
        RabbitMqChannelSource channelSource = new (connectionProvider);
        channelSource.SetChannelBudget(0, "no channel groups configured");

        return new RabbitMqTopology(
            "test",
            TopologyData.PrepareTopologyDataStructures(
                new Dictionary<Type, OutboundTarget>(),
                new Dictionary<string, OutboundTarget>(StringComparer.Ordinal),
                new Dictionary<string, InboundEndpoint>(StringComparer.Ordinal)
            ),
            RabbitMqCloudEventsTestFactory.CreateRegistry(),
            exchanges ?? [],
            queues ?? [],
            bindings ?? [],
            [],
            [],
            [],
            [],
            [],
            new Dictionary<InboundEndpointSelectionKey, RabbitMqInboundEndpoint>(),
            static _ => Task.CompletedTask,
            TimeSpan.FromSeconds(1),
            connectionProvider,
            channelSource
        );
    }
}
