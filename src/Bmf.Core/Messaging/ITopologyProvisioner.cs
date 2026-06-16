using System.Threading;
using System.Threading.Tasks;

namespace Bmf.Core.Messaging;

public interface ITopologyProvisioner
{
    Task ProvisionAsync(CancellationToken cancellationToken = default);
}
