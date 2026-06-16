using System.Threading.Tasks;

namespace Usf.Core.Messaging.Inbound;

public delegate Task MessageDelegate(IncomingMessageContext context);
