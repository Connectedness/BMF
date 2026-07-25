using System;
using BrilliantMessaging.Core.Messaging.Inbound;
using BrilliantMessaging.GuardClauses;

namespace BrilliantMessaging.Transport.Nats.Inbound;

/// <summary>
/// A NATS inbound endpoint.
/// </summary>
public sealed class NatsInboundEndpoint<TMessage> : NatsInboundEndpoint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NatsInboundEndpoint{TMessage}" /> class.
    /// </summary>
    public NatsInboundEndpoint(
        string name,
        string topologyName,
        string subject,
        Type handlerType,
        Type deserializerType,
        string discriminator,
        MessageDelegate handlerInvocation,
        MessageAckMode ackMode,
        RedeliveryClassifier redeliveryClassifier
    ) : base(
        name,
        topologyName,
        typeof(TMessage),
        handlerType,
        deserializerType,
        discriminator,
        handlerInvocation,
        ackMode,
        redeliveryClassifier
    )
    {
        // InboundEndpoint's constructor already ensured that handlerType is a concrete class.
        handlerType.MustBeAssignableTo(typeof(IMessageHandler<TMessage>));

        Subject = subject;
    }

    /// <summary>
    /// Gets the NATS source subject this endpoint handles.
    /// </summary>
    public override string Subject { get; }
}

/// <summary>
/// Non-generic NATS inbound endpoint base.
/// </summary>
public abstract class NatsInboundEndpoint : InboundEndpoint
{
    private protected NatsInboundEndpoint(
        string name,
        string topologyName,
        Type messageType,
        Type handlerType,
        Type deserializerType,
        string discriminator,
        MessageDelegate handlerInvocation,
        MessageAckMode ackMode,
        RedeliveryClassifier redeliveryClassifier
    ) : base(
        name,
        NatsTopology.TransportNameValue,
        topologyName,
        messageType,
        handlerType,
        deserializerType,
        discriminator,
        handlerInvocation,
        ackMode,
        redeliveryClassifier
    ) { }

    /// <summary>
    /// Gets the NATS source subject this endpoint handles.
    /// </summary>
    public abstract string Subject { get; }
}
