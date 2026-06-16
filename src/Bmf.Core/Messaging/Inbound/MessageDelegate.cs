using System.Threading.Tasks;

namespace Bmf.Core.Messaging.Inbound;

public delegate Task MessageDelegate(IncomingMessageContext context);
