using System.Threading;
using System.Threading.Tasks;

namespace Bmf.Core.Messaging.Inbound;

public interface IMessageHandler<in TMessage>
{
    Task HandleAsync(TMessage message, IncomingMessageContext context, CancellationToken cancellationToken);
}
