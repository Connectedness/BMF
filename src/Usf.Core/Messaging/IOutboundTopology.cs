using System;

namespace Usf.Core.Messaging;

public interface IOutboundTopology : IOutboundTargetRegistry
{
    OutboundTarget GetRequiredTarget(Type messageType);

    OutboundTarget<T> GetRequiredTarget<T>();

    bool TryGetTarget(Type messageType, out OutboundTarget? target);
}
