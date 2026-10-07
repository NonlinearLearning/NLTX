using System.Threading;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Captures one world's owner snapshots into a persistence document.
/// </summary>
/// <remarks>
/// Implementations must reject capture unless the source session has completed loading, is still
/// the host's active world, and the host world is ready; replaced, loading, failed, unpublished, or
/// reset-pending state is not saveable. Callers serialize capture against world load and
/// transformation through the shared world I/O gate.
/// </remarks>
public interface IWorldPersistenceSnapshotSource
{
  WorldPersistenceDocument Capture(CancellationToken cancellationToken);
}
