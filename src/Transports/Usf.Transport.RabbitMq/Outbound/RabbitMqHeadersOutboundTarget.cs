using System;
using System.Collections.Generic;
using Usf.Core.Messaging;
using Usf.Core.Messaging.Outbound;

namespace Usf.Transport.RabbitMq.Outbound;

public sealed class RabbitMqHeadersOutboundTarget<TMessage> : RabbitMqOutboundTarget<TMessage>
{
    private readonly IReadOnlyDictionary<string, object?> _headers;

    public RabbitMqHeadersOutboundTarget(
        string name,
        IMessageSerializer serializer,
        IMessageContractRegistry messageContractRegistry,
        string topologyName,
        RabbitMqOutboundChannelGroup channelGroup,
        string exchangeName,
        bool isMandatory,
        IReadOnlyDictionary<string, object?> headers
    )
        : base(name, serializer, messageContractRegistry, topologyName, channelGroup, exchangeName, isMandatory)
    {
        _headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }

    protected override IReadOnlyDictionary<string, object?> GetRawRouteHeaders()
    {
        return _headers;
    }

    protected override IReadOnlyDictionary<string, object?> GetRouteHeaders(TMessage message)
    {
        return _headers;
    }
}
