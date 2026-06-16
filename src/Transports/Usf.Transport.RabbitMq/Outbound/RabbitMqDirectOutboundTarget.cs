using System;
using Usf.Core.Messaging;
using Usf.Core.Messaging.Outbound;

namespace Usf.Transport.RabbitMq.Outbound;

public sealed class RabbitMqDirectOutboundTarget<TMessage> : RabbitMqRoutingKeyOutboundTarget<TMessage>
{
    public RabbitMqDirectOutboundTarget(
        string name,
        IMessageSerializer serializer,
        IMessageContractRegistry messageContractRegistry,
        string topologyName,
        RabbitMqOutboundChannelGroup channelGroup,
        string exchangeName,
        bool isMandatory,
        string? constantRoutingKey,
        Func<TMessage, string>? routingKeyFactory
    )
        : base(
            name,
            serializer,
            messageContractRegistry,
            topologyName,
            channelGroup,
            exchangeName,
            isMandatory,
            constantRoutingKey,
            routingKeyFactory
        ) { }
}
