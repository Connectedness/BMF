# BMF

*The Brilliant Messaging Framework*

[![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](https://github.com/Connectedness/BMF/blob/main/LICENSE)
[![NuGet](https://img.shields.io/badge/NuGet-0.1.0-blue.svg?style=for-the-badge)](https://www.nuget.org/packages?q=BMF)
[![Documentation](https://img.shields.io/badge/Docs-Changelog-yellowgreen.svg?style=for-the-badge)](https://github.com/Connectedness/BMF/releases)

BMF is the messaging framework that let's you keep control! No automatic, obscure generation of broker resources, no hidden dependencies, no magic. Define your topologies, publish messages and subscribe to them. Promotes CloudEvents. That's it!

## Why BMF?

Most messaging libraries try to be helpful by guessing what broker resources you want and conjuring them into existence at startup. That convenience becomes a liability the first time a typo silently provisions a phantom queue in production. BMF takes the opposite stance:

- **You declare, BMF provisions — nothing more.** Every exchange, queue, and binding is something you wrote down. There are no surprise resources on your broker.
- **CloudEvents are first-class, not bolted on.** Messages travel as [CloudEvents v1.0](https://cloudevents.io/) in *binary* content mode over AMQP 0.9.1. Interop is the default, not a serializer you have to remember to configure.
- **It lives inside the .NET host.** Configuration is a fluent chain off `IServiceCollection`; the runtime is driven by hosted services. If you know `Microsoft.Extensions.DependencyInjection` and `IHost`, you already know where BMF fits.
- **The whole API is yours.** BMF prefers `public` over `internal` — the extension points it uses internally are the same ones you can reach for. ([Public types, hidden in plain sight.](https://blog.ploeh.dk/2015/09/21/public-types-hidden-in-plain-sight/))

## Packages

All packages target `netstandard2.0`, so they happily light up on modern .NET as well as older runtimes.

| Package | What it gives you |
| --- | --- |
| `Bmf.Abstractions` | The CloudEvents contracts: `ICloudEvent` and `BaseCloudEvent`. |
| `Bmf.Core` | Publishing, consuming, message contracts, the topology model, and DI wiring. |
| `Bmf.Transport.RabbitMq` | The RabbitMQ transport — exchanges, queues, bindings, publishers, and consumers. |

## Installation

RabbitMQ is the only transport today, and it transitively references the other two packages. So a single reference is all you need:

```bash
dotnet add package Bmf.Transport.RabbitMq
```

## Quick start

A complete publish-and-consume loop is four small steps.

### 1. Define a message

A message *is* a CloudEvent. Inherit `BaseCloudEvent` and you get a retry-stable
`Id` (a time-ordered UUID) and `Time` for free:

```csharp
using Bmf.Abstractions;

public sealed record OrderPlaced(string OrderId, decimal Total) : BaseCloudEvent;
```

### 2. Register BMF and declare a topology

Map each message type to a CloudEvents `type` discriminator, then declare exactly
the broker resources you want. BMF will provision these — and only these — when the
host starts.

```csharp
using Bmf.Core.Messaging;
using Bmf.Transport.RabbitMq;
using RabbitMQ.Client;

builder
    .Services
    .AddBmf()
    .UseCloudEvents(options => options.Source = "/shop/orders")
    .MapMessageContracts(contracts =>
        contracts.Map<OrderPlaced>("shop.order.placed"))
    .AddRabbitMqTopology(rabbit =>
    {
        rabbit.UseConnectionFactory(_ => new ConnectionFactory
        {
            Uri = new Uri("amqp://guest:guest@localhost:5672")
        });

        rabbit.Exchange("orders", ExchangeType.Topic);
        rabbit.Queue("orders-processing");
        rabbit.QueueBinding("orders", "orders-processing", "shop.order.*");

        // Outbound: where OrderPlaced goes.
        rabbit.Publish<OrderPlaced>(target =>
            target.ToTopicExchange("orders", "shop.order.placed"));

        // Inbound: who handles it.
        rabbit.Consume("orders-processing", consumer =>
            consumer.Handle<OrderPlaced, OrderPlacedHandler>());
    });
```

`AddBmf` wires up two hosted services that run when your `IHost` starts — no extra
`StartAsync` calls on your part:

- a **provisioning** service that declares your exchanges, queues, and bindings on
  the broker (and only those), then validates the result, and
- a **runtime** service that opens the consumers and begins delivering messages to
  your handlers.

### 3. Publish

Inject `IMessagePublisher` and send. With no explicit target, BMF resolves one from
the topology by message type, and fills in the CloudEvents envelope (`id`, `time`,
`source`, `type`) from the message and your configured defaults.

```csharp
using Bmf.Core.Messaging.Outbound;

public sealed class Checkout(IMessagePublisher publisher)
{
    public Task PlaceAsync(OrderPlaced order, CancellationToken ct) =>
        publisher.PublishMessageAsync(order, cancellationToken: ct);
}
```

### 4. Handle

A handler implements `IMessageHandler<T>` and is resolved from a fresh DI scope per
delivery — so injecting scoped dependencies (a `DbContext`, say) just works.

```csharp
using Bmf.Core.Messaging.Inbound;

public sealed class OrderPlacedHandler : IMessageHandler<OrderPlaced>
{
    public Task HandleAsync(
        OrderPlaced message,
        IncomingMessageContext context,
        CancellationToken cancellationToken)
    {
        // ... process the order ...
        return Task.CompletedTask;
    }
}
```

That's the whole loop. The rest of this document explains what each moving part is
actually doing.

## Concepts

### Messages and CloudEvents

BMF publishes every message as a CloudEvent v1.0 in **binary** content mode: the
CloudEvents attributes ride in the AMQP headers and your payload is the raw body.
The two attributes the *call site* owns — `Id` and `Time` — are captured when the
message object is constructed and must stay stable across retries; regenerating them
mid-flight would turn a retry into a brand-new event. `BaseCloudEvent` enforces this
for you (`Id` defaults to a time-ordered `BmfUuid`, `Time` to `DateTimeOffset.UtcNow`),
but you can implement `ICloudEvent` directly when you need full control. The
application-wide `source` is set once via `UseCloudEvents`, and is validated at
startup so a missing or malformed `Source` fails fast rather than at first publish.

### Message contracts

A *message contract* maps a .NET type to the CloudEvents `type` discriminator that
identifies it on the wire. This is the one piece of bookkeeping BMF asks of you, and
it pays off in evolution-friendliness:

```csharp
contracts
    .Map<OrderPlaced>("shop.order.placed")
    .WithDataSchema("/schemas/order-placed/v1") // optional CloudEvents dataschema
    .WithInboundAlias("orders.placed");         // also accept a legacy discriminator
```

The same registry drives serialization on the way out and type resolution on the way
in. Aliases let a consumer keep accepting an old `type` value while publishers move
to a new one — schema evolution without breaking changes.

### Topologies

A **topology** is the heart of BMF. It is a named bundle of broker resources
(exchanges, queues, bindings), publishing targets, and consumers — and it owns
**exactly one connection to the broker**. Everything declared inside a topology
shares that connection's lifetime.

That one-connection rule is the lever behind BMF's most important production advice.
The [RabbitMQ production checklist](https://www.rabbitmq.com/docs/production-checklist#apps-connection-management)
recommends separating publishing and consuming onto different connections: when a
publishing connection gets throttled by broker flow control, a *shared* connection
would stall consumer acknowledgements at exactly the moment the broker needs
consumers to drain queues. BMF gives you three entry points to honour this:

- `AddRabbitMqTopology` — one topology carrying both publishers and consumers over a
  single shared connection. Ideal for low-traffic services and tests.
- `AddRabbitMqOutboundTopology` + `AddRabbitMqInboundTopology` — two topologies, each
  with its own dedicated connection. The recommended shape for production services.

Topologies are named (the dedicated outbound and inbound defaults are deliberately
different so they coexist without colliding), and provisioning happens once at
startup through a hosted service. If what's on the broker doesn't match what you
declared, you'll know immediately — not three deploys later.

### Publishing

`IMessagePublisher` is your outbound surface. The common call is
`PublishMessageAsync(message)`: BMF looks up the message type's **outbound target**
in the topology, builds the CloudEvent envelope, serializes the payload, and hands it
to the transport.

An *outbound target* is the routing decision you configured with `Publish<T>` — which
exchange, and how to route to it:

- `ToFanoutExchange(exchange)` — broadcast to every bound queue.
- `ToDirectExchange(exchange, routingKey)` — route by an exact key (fixed, or derived
  per message with a `Func<T, string>`).
- `ToTopicExchange(exchange, routingKey)` — route by a dot-delimited topic pattern.
- `ToHeadersExchange(exchange)` — route on `WithHeader(...)` values instead of a key.

You can register more than one target for the same type under different names (handy
when the same event fans out to several exchanges) and pick one explicitly at publish
time, or supply per-call CloudEvents metadata when you need to override the defaults
for a single send.

### Consuming

Consumers are configured per queue with `Consume`, and a single queue can dispatch to
several typed handlers — BMF inspects each delivery's `type`, resolves the matching
`IMessageHandler<T>` from a per-delivery DI scope, and invokes it:

```csharp
rabbit
    .Consume("orders-processing", consumer => consumer
    .PrefetchCount(20)   // how many unacknowledged messages the broker may push
    .ChannelCount(4)     // spread deliveries across this many channels for parallelism
    .Handle<OrderPlaced, OrderPlacedHandler>()
    .Handle<OrderCancelled, OrderCancelledHandler>());
```

`PrefetchCount` sets the per-consumer QoS window, `ChannelCount` scales out across
channels, and `Concurrency` controls how many deliveries a single channel dispatches
in parallel. Handler types are auto-registered as scoped; register the concrete type
yourself beforehand if you want a different lifetime.

### Acknowledgements

By default (`MessageAckMode.Auto`) BMF acknowledges a message once the handler
returns and negatively acknowledges it if the handler throws — the right behaviour for
most work. When you need the acknowledgement to hinge on something the handler does —
deferring it until downstream work has committed, for instance — switch a handler to
`MessageAckMode.Manual` and drive it yourself through the `IMessageAcknowledgement`
handle on `IncomingMessageContext`.

### Publisher confirms and mandatory routing

How sure do you need to be that a publish landed? That's the question
`RabbitMqPublisherConfirmMode` answers, configured per channel group:

- **`FireAndForget`** — publish and move on. Fastest, but a broker nack or an
  unroutable message disappears silently.
- **`Confirms`** — wait for the broker to confirm each publish, surfacing nacks and
  unroutable returns as a `MessageDeliveryException`.

Marking a target `Mandatory()` asks the broker to reject a message it can't route to
any queue. Because that rejection comes back asynchronously, BMF needs publisher
confirms to correlate it — so a mandatory target on a `FireAndForget` group is
rejected at build time with a `TopologyValidationException` rather than failing
mysteriously at runtime. Confirmation tracking serializes outstanding publishes per
channel (preserving order at one round-trip per publish); widen the channel group when
you'll trade strict ordering for throughput.

### Channel pooling and channel groups

Channels are not connections, but they aren't free either, and RabbitMQ frowns on
sharing one channel across threads. BMF manages a pool of channels over the topology's
single connection so your code never touches a raw `IChannel`. **Channel groups** are
how you tune that pool: each group has a maximum channel count and, on the outbound
side, its own publisher-confirm settings. Point a target at a named group with
`UseChannelGroup` to give a hot path its own dedicated, independently-tuned channels —
or just lean on the implicit defaults until a benchmark tells you otherwise.

### Reliability and recovery

The RabbitMQ transport requires RabbitMQ.Client's automatic connection recovery
(`ConnectionFactory.AutomaticRecoveryEnabled = true`) and validates it at startup.
RabbitMQ.Client owns reconnection; BMF keeps using the same auto-recovering connection
for the topology's lifetime. Topology recovery (`TopologyRecoveryEnabled`, on by
default) restores exchanges, queues, and bindings — required for inbound topologies so
consumer subscriptions come back after a blip, and safe to disable when you provision
broker topology externally.

One honest caveat: automatic recovery is an *availability* mechanism, not a delivery
guarantee. It does not buffer or replay messages that were in flight during an outage.
If you need at-least-once effects, make your publishes safe to retry or put an outbox
in front of them.

## License

BMF is licensed under the [MIT License](LICENSE).
