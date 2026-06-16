using Usf.Core.Messaging;
using Usf.Core.Messaging.Outbound;

namespace Usf.Transport.RabbitMq.Outbound;

public sealed class RabbitMqFanoutOutboundTarget<TMessage> : RabbitMqOutboundTarget<TMessage>
{
    public RabbitMqFanoutOutboundTarget(
        string name,
        IMessageSerializer serializer,
        IMessageContractRegistry messageContractRegistry,
        string topologyName,
        RabbitMqOutboundChannelGroup channelGroup,
        string exchangeName,
        bool isMandatory
    )
        : base(name, serializer, messageContractRegistry, topologyName, channelGroup, exchangeName, isMandatory) { }
}
