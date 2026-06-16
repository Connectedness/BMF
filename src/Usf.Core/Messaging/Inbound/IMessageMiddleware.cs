using System.Threading.Tasks;

namespace Usf.Core.Messaging.Inbound;

public interface IMessageMiddleware
{
    Task InvokeAsync(IncomingMessageContext context, MessageDelegate next);
}
