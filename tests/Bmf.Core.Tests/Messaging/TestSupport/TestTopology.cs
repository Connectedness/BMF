using System;
using System.Collections.Generic;
using Bmf.Core.Messaging;
using Bmf.Core.Messaging.Inbound;
using Bmf.Core.Messaging.Outbound;

namespace Bmf.Core.Tests.Messaging.TestSupport;

public sealed class TestTopology : Topology
{
    public TestTopology(
        string name,
        IDictionary<Type, OutboundTarget>? targetsByMessageType = null,
        IDictionary<string, OutboundTarget>? targetsByName = null,
        IDictionary<string, InboundEndpoint>? endpointsByName = null
    ) : base(
        name,
        TopologyData.PrepareTopologyDataStructures(
            targetsByMessageType ?? new Dictionary<Type, OutboundTarget>(),
            targetsByName ?? new Dictionary<string, OutboundTarget>(StringComparer.Ordinal),
            endpointsByName ?? new Dictionary<string, InboundEndpoint>(StringComparer.Ordinal)
        )
    ) { }
}
