using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>World-side validation for packet 125; the player owner supplies the authenticated slot.</summary>
public readonly record struct WorldPickingIntent(
    ulong WorldEpoch,
    CommitVersion Version,
    byte PlayerSlot,
    TileCoordinate Coordinate,
    byte TileStyle);

public sealed partial class WorldOwner
{
  /// <summary>Creates a version-bound picking relay intent without persisting player position.</summary>
  public bool TryCreatePickingIntent(
      ulong worldEpoch,
      byte playerSlot,
      TileCoordinate coordinate,
      byte tileStyle,
      out WorldPickingIntent intent)
  {
    lock (_gate)
    {
      intent = default;
      if (!IsCurrentWorld(worldEpoch) || !_header!.Geometry.Contains(coordinate))
      {
        return false;
      }

      intent = new WorldPickingIntent(
          _worldEpoch,
          CurrentVersion,
          playerSlot,
          coordinate,
          tileStyle);
      return true;
    }
  }
}
