using System;
using System.IO;
using System.Threading;
using NSSLC.WorldGeneration;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldGeneration.Components;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;
using WorldLoadRecoveryPhase = Terraria.WorldGeneration.Components.WorldLoadRecoveryPhase;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>Commits the legacy TileEntity table before capturing a session save.</summary>
public sealed class LegacyWorldTileEntitySavePreparation : IWorldSaveSnapshotPreparation
{
  private const int WorldEdgeClearance = 1;
  private readonly LoadedWorldSession _session;
  private readonly int _ownerThreadId;

  public LegacyWorldTileEntitySavePreparation(LoadedWorldSession session)
  {
    _session = session ?? throw new ArgumentNullException(nameof(session));
    _ownerThreadId = Environment.CurrentManagedThreadId;
  }

  public WorldStorageOperationResult Prepare(CancellationToken cancellationToken)
  {
    try
    {
      EnsureOwnerThread();
      if (!_session.IsComplete || !_session.IsPublished || _session.IsPublicationUncertain ||
          _session.Lifecycle.RecoveryPhase != WorldLoadRecoveryPhase.Completed ||
          _session.Lifecycle.IsGeneratingOrLoadingWorld ||
          _session.Lifecycle.LoadFailed ||
          _session.Lifecycle.RequiresWorldReset)
      {
        return Invalid("A runtime TileEntity snapshot requires a settled published session.");
      }

      if (Main.tile is null || Main.maxTilesX != _session.Storage.TileMap.Width ||
          Main.maxTilesY != _session.Storage.TileMap.Height ||
          Main.tile.GetLength(0) != _session.Storage.TileMap.Width ||
          Main.tile.GetLength(1) != _session.Storage.TileMap.Height)
      {
        return Invalid("The runtime tile map does not match the session being saved.");
      }

      cancellationToken.ThrowIfCancellationRequested();
      TileEntityStoreSnapshot snapshot = TileEntity.CreatePersistenceSnapshot();
      if (snapshot.Entities.Count > WorldFileTileEntityCodec.MaxEntityCount)
      {
        return Invalid("The runtime TileEntity count exceeds the WorldFile record limit.");
      }
      if (snapshot.NextId < _session.Storage.TileEntities.NextId)
      {
        return Invalid("The runtime TileEntity next ID is older than the session state.");
      }

      int endAllowedX = _session.Storage.TileMap.Width - WorldEdgeClearance;
      int endAllowedY = _session.Storage.TileMap.Height - WorldEdgeClearance;
      foreach (TileEntitySnapshot entity in snapshot.Entities)
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (entity.Anchor.X < WorldEdgeClearance || entity.Anchor.X >= endAllowedX ||
            entity.Anchor.Y < WorldEdgeClearance || entity.Anchor.Y >= endAllowedY)
        {
          return Invalid("A runtime TileEntity anchor is outside the WorldFile world bounds.");
        }
      }

      cancellationToken.ThrowIfCancellationRequested();
      _session.Storage.TileEntities.CommitRuntimeSnapshot(snapshot);
      _session.Storage.TileEntityUpdates.Replace(
        _session.Storage.TileEntities.CreateScheduledIdSnapshot());
      return WorldStorageOperationResult.Success;
    }
    catch (OperationCanceledException exception)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message));
    }
    catch (InvalidDataException exception)
    {
      return Invalid(exception.Message);
    }
    catch (ArgumentException exception)
    {
      return Invalid(exception.Message);
    }
    catch (InvalidOperationException exception)
    {
      return Invalid(exception.Message);
    }
    catch (OverflowException exception)
    {
      return Invalid(exception.Message);
    }
  }

  private void EnsureOwnerThread()
  {
    if (Environment.CurrentManagedThreadId != _ownerThreadId)
    {
      throw new InvalidOperationException(
        "Runtime TileEntity snapshots must commit on their owner thread.");
    }
  }

  private static WorldStorageOperationResult Invalid(string detail)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, detail));
  }
}
