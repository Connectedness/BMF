using System.Threading;
using System.Threading.Tasks;

namespace Usf.Core.Messaging;

public interface IMessageSerializer
{
    ValueTask<CloudEventEnvelope> SerializeAsync<T>(
        T message,
        in CloudEventMetadata metadata,
        CancellationToken cancellationToken = default
    );
}
