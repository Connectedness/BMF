using System.Threading;
using System.Threading.Tasks;

namespace Usf.Core.Messaging;

public interface IMessagePublisher
{
    Task PublishMessageAsync<T>(
        T message,
        OutboundTarget? target = null,
        CancellationToken cancellationToken = default
    );

    Task PublishRawAsync(
        SerializedMessage message,
        OutboundTarget target,
        CancellationToken cancellationToken = default
    );
}
