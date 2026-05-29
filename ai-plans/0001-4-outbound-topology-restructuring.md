## Rationale

For USF, we want to introduce two new terms: Outbound Topology and Inbound Topology. The former addresses everything regarding sending/publishing messages, the latter everything related to consuming messages.

USF should make outbound messaging topology explicit before inbound messaging is added. The core publishing model should use outbound terminology, with `OutboundTarget` as the message-type-specific executable route. RabbitMQ should expose addresses and channel groups as first-class outbound topology concepts while keeping broker details visible and configurable. A `RabbitMqOutboundTopology` should own exactly one outbound RabbitMQ connection, its channel groups, compiled outbound targets, provisioning model, and disposal lifecycle; the separate `RabbitMqConnectionManager` concept should be removed.

## Acceptance Criteria

- [ ] Core publishing types are renamed to outbound terminology, including `Target` to `OutboundTarget`, `Target<T>` to `OutboundTarget<T>`, `IMessageTopology` to `IOutboundTopology`, `MessageTopology` to `OutboundTopology`, `ITargetRegistry` to `IOutboundTargetRegistry`, `ITopologyProvisioner` to `IOutboundTopologyProvisioner`, and related exception, hosted-service, diagnostic, and test names.
- [ ] RabbitMQ publishing types are renamed to outbound terminology, including the builder, configuration, compiler, compiled topology, provisioner, target base class, and concrete target classes.
- [ ] `RabbitMqConnectionManager` is removed as a separate type, and `RabbitMqOutboundTopology` owns one outbound RabbitMQ connection directly, including lazy creation, `channel_max` validation, and disposal.
- [ ] RabbitMQ addresses are introduced as first-class outbound topology definitions, so targets reference an address rather than an exchange directly.
- [ ] Multiple outbound targets can publish to the same RabbitMQ address while retaining independent serializers, routing-key/header behavior, names, and message types.
- [ ] `IMessagePublisher` supports publishing an already prepared `SerializedMessage` through an explicit non-generic `OutboundTarget` without invoking USF serialization.
- [ ] `PublishUntypedAsync(object, ...)` is removed; non-generic outbound dispatch uses a serialized-payload method and typed/object mismatch handling stays in the publisher.
- [ ] RabbitMQ channel groups are introduced as first-class outbound topology definitions, so two or more targets can intentionally share a channel pool.
- [ ] The current per-target/shared channel-pooling mode is replaced by channel groups while preserving the default behavior of one private single-channel group per target.
- [ ] `RabbitMqOutboundTopology` owns all channel groups and disposes channel pools before disposing the outbound RabbitMQ connection.
- [ ] RabbitMQ exchange and queue declaration remains explicit and uses `RabbitMqDeclareMode` with `Skip`, `Passive`, and `Active` values.
- [ ] RabbitMQ binding configuration uses `RabbitMqBindingMode` with `Skip` and `Active` values, because RabbitMQ has no passive binding declaration.
- [ ] Outbound topology validation rejects duplicate targets, duplicate addresses, duplicate channel groups, unknown address references, unknown channel group references, address references to unknown exchanges, missing serializers, invalid exchange/route combinations, invalid channel group sizes, and channel budgets that exceed the broker's negotiated `channel_max`.
- [ ] Outbound topology tests and RabbitMQ integration tests are updated for the renamed API, shared addresses, channel groups, passive and active exchange declaration, and connection disposal order.

## Technical Details

The core namespace should move from message-publishing terminology toward outbound topology terminology. `OutboundTarget` remains the non-generic abstraction used by topology dictionaries, explicit target lookup, diagnostics, explicit target passing, and raw outbound payload dispatch. `OutboundTarget<T>` remains the typed hot path and continues to own the serializer and dispatch logic for one message type. `IMessagePublisher` should remain focused on publish semantics and depend on `IOutboundTopology` and `OutboundTarget`. Sending and request-response should be introduced as separate outbound-facing abstractions in future slices, such as `IMessageSender` and a request-response client, instead of widening the publisher abstraction.

`IMessagePublisher` should expose a public raw publish overload that accepts an already prepared `SerializedMessage` and an explicit non-generic `OutboundTarget`. This overload must bypass USF serialization and dispatch the supplied serialized payload directly through the target. The typed `PublishMessageAsync<T>` path remains the default application-message API and should continue to use `OutboundTarget<T>` when possible to avoid boxing and keep serializer ownership on the target.

Raw publishing must require an explicit `OutboundTarget`; it must not attempt default topology resolution because an already serialized message has no CLR message type that can be used for target lookup. `OutboundTarget` should not require a CLR message type. If a type indicator is useful for diagnostics, expose it as nullable metadata or only on `OutboundTarget<T>`.

The old `PublishUntypedAsync(object message, ...)` method should be removed from the target abstraction. The non-generic `OutboundTarget` should instead expose serialized-payload dispatch, for example `PublishSerializedAsync(SerializedMessage message, CancellationToken cancellationToken = default)`. `OutboundTarget<T>` should serialize typed messages and then call the serialized-payload dispatch method. `IMessagePublisher` should perform the bridge decision: if the resolved or explicit target is `OutboundTarget<T>`, use the typed path; otherwise throw an outbound target type-mismatch exception instead of passing the typed message as `object`. This keeps the struct hot path unboxed and separates typed object publishing from raw serialized payload publishing.

A concrete sketch of the target shape is:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Usf.Core.Messaging;

public abstract class OutboundTarget
{
    public string Name { get; }

    public string TransportName { get; }

    public virtual Type? MessageType => null;

    public abstract Task PublishSerializedAsync(
        SerializedMessage message,
        CancellationToken cancellationToken = default);
}

public abstract class OutboundTarget<T> : OutboundTarget
{
    public sealed override Type MessageType => typeof(T);

    public async Task PublishAsync(T message, CancellationToken cancellationToken = default)
    {
        // ...
    }
}
```

`OutboundTopology` should keep the current two lookup surfaces: one default target per exact message type and named targets through `IOutboundTargetRegistry`. The default publish path remains exact-type resolution. Named targets remain the escape hatch for additional explicit routes for the same message type.

The RabbitMQ configuration model should separate broker entities, addresses, targets, and channel groups. Exchanges, queues, and bindings remain RabbitMQ broker entities and keep their explicit declaration or binding settings. Outbound topology should not artificially forbid queue or binding declaration, because declaration is operational broker setup and some applications need to manage all required broker entities from one composition root. Documentation should still teach the intended split: outbound topologies should usually target exchanges through outbound addresses, while inbound topologies should usually target exchanges, queues, and bindings as part of receive endpoint setup. A new `RabbitMqAddressDefinition` should represent an outbound destination such as an exchange-backed address. In this slice, an address can be RabbitMQ-specific and contain the referenced exchange name. Publish target configuration should reference `AddressName` instead of `ExchangeName`; concrete target compilation resolves the address to its exchange during validation and compile.

Channel groups should replace `RabbitMqChannelPoolingMode`, `MaxChannelsPerTarget`, and `SharedChannelPoolSize` as the primary configuration model. A `RabbitMqChannelGroupDefinition` should at least contain a name and maximum channel count. Targets may reference a channel group by name. If a target does not specify a channel group, the compiler creates an implicit private single-channel group for that target, so existing per-target ordering behavior remains the default. Explicitly named channel groups allow multiple targets to share the same bounded pool. Worst-case channel count is the sum of distinct channel group maximums.

`RabbitMqOutboundTopology` should own the outbound connection directly. It should accept the configured `ConnectionFactory` factory, lazily create the RabbitMQ connection, validate the channel budget against `IConnection.ChannelMax` on first connection, expose the connection to channel groups through a narrow private method or delegate, and dispose channel groups before disposing the connection. The old `RabbitMqConnectionManager` type should be deleted.

Provisioning should be renamed to outbound topology provisioning and operate from `RabbitMqOutboundTopology`. For this outbound-focused plan, exchange, queue, queue-binding, and exchange-binding provisioning may all remain available from the RabbitMQ outbound builder. Outbound targets should still publish only through addresses, and RabbitMQ addresses should resolve to exchanges in this slice. Queues, dead-lettering, skipped messages, prefetch, concurrency, consumers, and receive endpoint behavior remain inbound topology concerns even though broker entity declaration is available in both topology builders.

RabbitMQ entity configuration should replace `RabbitMqDeclareMode.None` with `RabbitMqDeclareMode.Skip`, replace `RabbitMqDeclareMode.Ensure` with `RabbitMqDeclareMode.Active`, and keep `RabbitMqDeclareMode.Passive` for exchange and queue passive checks. Binding configuration should replace `RabbitMqBindingDeclareMode` with `RabbitMqBindingMode`, containing only `Skip` and `Active`. Builder method names should avoid calling binding behavior passive declaration; RabbitMQ binds, and active binding is idempotent broker setup.

The compiler should validate the full configuration deterministically before building runtime objects. Validation should group and sort errors consistently, including duplicate address and channel group names, missing referenced addresses or groups, address references to missing exchanges, route/exchange-type mismatches, duplicate default targets, duplicate named targets, missing or unregistered serializers, invalid channel group sizes, unsupported declaration or binding modes, and channel budget overflow. Broker-state incompatibilities that cannot be proven from configuration should continue to fail during passive or active provisioning.

Tests should follow the renamed API and cover the new model sociably through the RabbitMQ builder where possible. Unit tests should verify address sharing, channel group sharing, implicit private channel groups, channel budget calculation, and disposal order. Integration tests should publish multiple message types through the same address and verify routing outcomes, and should cover both passive and active exchange declaration behavior.
