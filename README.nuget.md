# BMF — The Brilliant Messaging Framework

*Explicit messaging. No magic.*

BMF is a lightweight, CloudEvents-first messaging framework for modern cloud-native
.NET applications. You declare your broker topology; BMF provisions exactly those
resources — no surprise queues, no hidden dependencies. Messages travel as
[CloudEvents v1.0](https://cloudevents.io/) in binary content mode over RabbitMQ
(AMQP 0.9.1), and the whole thing lives inside the .NET generic host.

## Packages

| Package | What it gives you |
| --- | --- |
| `Bmf.Abstractions` | The CloudEvents contracts: `ICloudEvent` and `BaseCloudEvent`. |
| `Bmf.Core` | Publishing, consuming, message contracts, the topology model, and DI wiring. |
| `Bmf.Transport.RabbitMq` | The RabbitMQ transport — exchanges, queues, bindings, publishers, and consumers. |
| `Bmf.OpenTelemetry` | One-line registration of BMF's tracing and metrics with the OpenTelemetry SDK. |

RabbitMQ is the only transport today and transitively references the other two
packages, so a single reference is all you need:

```bash
dotnet add package Bmf.Transport.RabbitMq
```

## Quick start

A complete publish-and-consume loop is four small steps.

```csharp
using Bmf.Abstractions;
using Bmf.Core.Messaging;
using Bmf.Core.Messaging.Inbound;
using Bmf.Core.Messaging.Outbound;
using Bmf.Transport.RabbitMq;
using RabbitMQ.Client;

// 1. A message *is* a CloudEvent.
public sealed record OrderPlaced(string OrderId, decimal Total) : BaseCloudEvent;

// 2. Register BMF and declare exactly the broker resources you want.
builder.Services
    .AddBmf()
    .UseCloudEvents(options => options.Source = "/shop/orders")
    .MapMessageContracts(contracts => contracts.Map<OrderPlaced>("shop.order.placed"))
    .AddRabbitMqTopology(rabbit =>
    {
        rabbit.UseConnectionFactory(_ => new ConnectionFactory
        {
            Uri = new Uri("amqp://guest:guest@localhost:5672")
        });

        rabbit.Exchange("orders", ExchangeType.Topic);
        rabbit.Queue("orders-processing");
        rabbit.QueueBinding("orders", "orders-processing", "shop.order.*");

        rabbit.Publish<OrderPlaced>(target =>
            target.ToTopicExchange("orders", "shop.order.placed"));

        rabbit.Consume("orders-processing", consumer =>
            consumer.Handle<OrderPlaced, OrderPlacedHandler>());
    });

// 3. Publish — inject IMessagePublisher and send.
public sealed class Checkout(IMessagePublisher publisher)
{
    public Task PlaceAsync(OrderPlaced order, CancellationToken ct) =>
        publisher.PublishMessageAsync(order, cancellationToken: ct);
}

// 4. Handle — resolved from a fresh DI scope per delivery.
public sealed class OrderPlacedHandler : IMessageHandler<OrderPlaced>
{
    public Task HandleAsync(OrderPlaced message, IncomingMessageContext context, CancellationToken ct)
    {
        // ... process the order ...
        return Task.CompletedTask;
    }
}
```

`AddBmf` wires up the hosted services that provision your topology and drive the
consumers when your `IHost` starts — no extra `StartAsync` calls on your part.

## Documentation

Concepts, outbound targets and routing keys, the customizable inbound pipeline,
acknowledgements, publisher confirms, channel groups, reliability, and the
observability guide all live in the full README on GitHub:

**👉 https://github.com/Connectedness/BMF**

## License

BMF is licensed under the [MIT License](https://github.com/Connectedness/BMF/blob/main/LICENSE).
