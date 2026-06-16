using System.Threading;
using System.Threading.Tasks;

namespace Bmf.Core.Messaging.Inbound;

public interface IInboundMessageInspector
{
    ValueTask<InboundMessageInspectionResult> InspectAsync(
        TransportMessage transportMessage,
        CancellationToken cancellationToken = default
    );
}
