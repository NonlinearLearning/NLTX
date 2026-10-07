using System.Threading;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Supplies world-owned work that must run around owner API loading.
/// </summary>
public interface IWorldLoadRecoveryEffects
{
  /// <summary>
  /// Atomically makes the fully committed session the active world before load-time simulation
  /// effects run. A failed result is treated as a potentially partial publication and reset.
  /// </summary>
  WorldStorageOperationResult PublishLoadedWorld(
    LoadedWorldSession session,
    CancellationToken cancellationToken);

  /// <summary>
  /// Performs liquid settling and water checks before the legacy load gate is released.
  /// </summary>
  WorldStorageOperationResult SettleLoadedWorld(
    LoadedWorldSession session,
    CancellationToken cancellationToken);

  /// <summary>
  /// Completes NPC and world-specific finalization after the legacy load gate is released.
  /// </summary>
  WorldStorageOperationResult FinalizeLoadedWorld(
    LoadedWorldSession session,
    CancellationToken cancellationToken);

  /// <summary>
  /// Clears or rebuilds all world-owned state after a partial owner commit.
  /// </summary>
  WorldStorageOperationResult ResetWorld(LoadedWorldSession session);
}
