using System.Threading;
using System.Threading.Tasks;

namespace MinimalImplementation.Persistence;

/// <summary>Host actions needed to drain, save and terminate one world process.</summary>
public interface IWorldHostStopBoundary
{
  Task StopAdmissionsAsync(CancellationToken cancellationToken);

  Task StopTickAdmissionAsync(CancellationToken cancellationToken);

  Task DrainAcceptedCommandsAsync(CancellationToken cancellationToken);

  Task CloseTransportAsync(CancellationToken cancellationToken);

  Task RetireWorldAndResourcesAsync(CancellationToken cancellationToken);

  Task StopCommitLoopAsync(CancellationToken cancellationToken);
}
