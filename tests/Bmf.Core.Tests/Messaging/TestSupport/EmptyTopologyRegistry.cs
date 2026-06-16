using System;
using System.Collections.Generic;
using Bmf.Core.Messaging;

namespace Bmf.Core.Tests.Messaging.TestSupport;

public sealed class EmptyTopologyRegistry : ITopologyRegistry
{
    public IReadOnlyCollection<string> Names { get; } = [];

    public Topology GetRequiredTopology(string name)
    {
        throw new InvalidOperationException(
            $"Topology '{name}' is not registered. Registered topologies: {TopologyRegistrationCatalog.FormatNames(Names)}."
        );
    }

    public bool TryGetTopology(string name, out Topology? topology)
    {
        topology = null;
        return false;
    }
}
