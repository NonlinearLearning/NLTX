using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

public sealed partial class WorldOwner
{
  /// <summary>
  /// Reads one tile only when the caller presents the epoch and revision it captured. This keeps
  /// combat, inventory and mechanism queries from silently mixing world versions.
  /// </summary>
  public bool TryReadTile(
      ulong worldEpoch,
      CommitVersion version,
      TileCoordinate coordinate,
      out WorldTile tile)
  {
    lock (_gate)
    {
      tile = default;
      if (!IsCurrentWorld(worldEpoch) || version != CurrentVersion ||
          !_header!.Geometry.Contains(coordinate))
      {
        return false;
      }

      tile = _tiles![GetTileIndex(coordinate)];
      return true;
    }
  }

  /// <summary>Returns the current revision without exposing mutable owner state.</summary>
  public bool TryGetCurrentRevision(out ulong worldEpoch, out CommitVersion version)
  {
    lock (_gate)
    {
      if (!IsReady)
      {
        worldEpoch = 0;
        version = default;
        return false;
      }

      worldEpoch = _worldEpoch;
      version = CurrentVersion;
      return true;
    }
  }
}
