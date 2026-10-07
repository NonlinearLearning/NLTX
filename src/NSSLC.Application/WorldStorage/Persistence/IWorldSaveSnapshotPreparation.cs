using System.Threading;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>Commits host-owned state needed by a save snapshot.</summary>
public interface IWorldSaveSnapshotPreparation
{
  WorldStorageOperationResult Prepare(CancellationToken cancellationToken);
}
