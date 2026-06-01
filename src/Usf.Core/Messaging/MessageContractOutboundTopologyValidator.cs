using System;
using System.Collections.Generic;
using Usf.Core.Messaging.Errors;

namespace Usf.Core.Messaging;

public sealed class MessageContractOutboundTopologyValidator : IOutboundTopologyValidator
{
    private readonly IMessageContractRegistry _messageContractRegistry;
    private readonly IOutboundTopology _outboundTopology;

    public MessageContractOutboundTopologyValidator(
        IOutboundTopology outboundTopology,
        IMessageContractRegistry messageContractRegistry
    )
    {
        _outboundTopology = outboundTopology ?? throw new ArgumentNullException(nameof(outboundTopology));
        _messageContractRegistry = messageContractRegistry ??
                                   throw new ArgumentNullException(nameof(messageContractRegistry));
    }

    public void Validate()
    {
        List<string> validationErrors = [];

        foreach (var target in _outboundTopology.Targets)
        {
            if (target.MessageType is null)
            {
                continue;
            }

            try
            {
                _ = _messageContractRegistry.GetDiscriminator(target.MessageType);
            }
            catch (MessageContractNotRegisteredException)
            {
                validationErrors.Add(
                    $"Outbound target '{target.Name}' publishes unregistered CloudEvents message type '{target.MessageType}'. Register its canonical discriminator with MessageContractRegistryBuilder.Map<T>(...) or MapOutbound<T>(...)."
                );
            }
        }

        if (validationErrors.Count > 0)
        {
            throw new OutboundTopologyValidationException(validationErrors);
        }
    }
}
