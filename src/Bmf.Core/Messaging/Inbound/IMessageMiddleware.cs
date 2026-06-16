using System.Threading.Tasks;

namespace Bmf.Core.Messaging.Inbound;

public interface IMessageMiddleware
{
    Task InvokeAsync(IncomingMessageContext context, MessageDelegate next);
}
