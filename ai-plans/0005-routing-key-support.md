# Routing Key Support

## Rationale

Allow callers to pass a domain-driven routing key when publishing a message, treating it as an optional parameter for an already selected outbound target. Routing keys must stay simple `string` values and must not become a transport-specific DSL or replace `OutboundTarget` selection.

## Acceptance Criteria

- [ ] `IMessagePublisher.PublishMessageAsync` overloads accept an optional routing key in addition to the optional `OutboundTarget` without introducing ambiguous positional string calls for topology selection.
- [ ] Topology-scoped publishing through `TopologyPublisher` supports the same optional routing key behavior.
- [ ] `OutboundTarget<T>`'s typed publish contract (`PublishAsync` / `PublishTypedCloudEventAsync`) carries the optional routing key; the non-generic `OutboundTarget.PublishSerializedAsync` raw path remains unchanged.
- [ ] `PublishRawAsync` remains unchanged because `SerializedMessage` already carries routing information in its headers.
- [ ] Routing keys are represented as `string` values and remain optional.
- [ ] Routing-key interpretation remains transport-specific; the core contract carries the routing key without encoding broker-specific routing rules.
- [ ] For RabbitMQ typed publishing, a non-null caller routing key overrides both constant target routing keys and message-derived routing-key factories; when omitted, existing target routing behavior is preserved.
- [ ] Existing target-based publishing behavior continues to work when no routing key is provided.
- [ ] Automated tests need to be written.

## Technical Details

Extend the core typed publishing flow by adding optional `string? routingKey = null` parameters to the public `IMessagePublisher.PublishMessageAsync` overloads and the `TopologyPublisher` forwarding methods. Thread the routing key through `MessagePublisher`'s public/default-topology overloads, its core publish path, `OutboundTarget<T>.PublishAsync`, `OutboundTarget<T>.PublishCoreAsync`, and `PublishTypedCloudEventAsync` so every transport receives caller routing information without changing target resolution semantics. Keep topology-scoped `MessagePublisher` implementation overloads explicit about their API shape: do not introduce a positional `string` routing-key slot that can be confused with `TopologyName` via implicit conversion, and preserve existing positional cancellation-token calls.

Routing keys are a core outbound-target concept, not a RabbitMQ-specific feature. The core messaging layer should carry the optional routing key as a simple `string` and leave the meaning of that value to transport implementations. Transport-specific code decides how a routing key affects delivery for that broker.

Do not change `PublishRawAsync`; it is the only `IMessagePublisher` method that accepts `SerializedMessage` directly, and serialized messages already carry routing information in their headers.

For RabbitMQ, thread the supplied typed-publish routing key into `RabbitMqOutboundTarget<TMessage>` dispatch. The effective AMQP routing key should be the caller-supplied key when non-null, otherwise the target's existing routing key behavior. This means a caller routing key overrides both constant direct/topic target keys and message-derived routing-key factories. When a caller routing key is supplied, do not evaluate the target's constant routing-key path or message-derived routing-key factory; evaluate existing target routing behavior only when the caller routing key is null. Keep route headers separate from routing keys.

Update unit tests around `MessagePublisher`, `TopologyPublisher`, and RabbitMQ outbound targets to cover routing key propagation, default behavior when omitted, explicit target behavior, unchanged raw publishing, and the RabbitMQ override/default distinction. Extend `TestRabbitMqChannel` so RabbitMQ unit tests can observe the `routingKey` argument passed to `BasicPublishAsync`.
