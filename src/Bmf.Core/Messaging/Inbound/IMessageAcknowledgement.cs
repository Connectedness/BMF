using System.Threading;
using System.Threading.Tasks;

namespace Bmf.Core.Messaging.Inbound;

public interface IMessageAcknowledgement
{
    Task AckAsync(CancellationToken cancellationToken = default);

    Task NackAsync(bool requeue, CancellationToken cancellationToken = default);
}
