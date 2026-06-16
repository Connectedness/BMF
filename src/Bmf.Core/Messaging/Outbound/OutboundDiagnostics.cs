using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Bmf.Core.Messaging.Outbound;

public static class OutboundDiagnostics
{
    public const string ActivitySourceName = "Bmf.Outbound";

    public const string MessageTypeTagName = "bmf.outbound.message.type";

    public const string TargetNameTagName = "bmf.outbound.target.name";

    public const string TransportNameTagName = "bmf.outbound.transport.name";

    public const string OutcomeTagName = "bmf.outbound.outcome";

    public const string DeliveryFailureReasonTagName = "bmf.outbound.delivery.failure.reason";

    public static readonly ActivitySource ActivitySource = new (ActivitySourceName);

    public static readonly Meter Meter = new (ActivitySourceName);

    public static readonly Counter<long> PublishAttempts = Meter.CreateCounter<long>("bmf.outbound.publish.attempts");

    public static readonly Counter<long> PublishFailures = Meter.CreateCounter<long>("bmf.outbound.publish.failures");

    public static readonly Histogram<double> PublishDuration =
        Meter.CreateHistogram<double>("bmf.outbound.publish.duration", unit: "ms");

    public static readonly Counter<long> TopologyProvisioningAttempts =
        Meter.CreateCounter<long>("bmf.outbound.topology.provisioning.attempts");

    public static readonly Counter<long> TopologyProvisioningFailures =
        Meter.CreateCounter<long>("bmf.outbound.topology.provisioning.failures");

    public static readonly Histogram<double> TopologyProvisioningDuration = Meter.CreateHistogram<double>(
        "bmf.outbound.topology.provisioning.duration",
        unit: "ms"
    );
}
