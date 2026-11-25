using Terraria.Share.Items;

namespace Terraria.Unshared.World;

/// <summary>One decoded tile entity: identity, kind and the payload the runtime keeps.</summary>
/// <remarks>
/// Version4 stores a class hierarchy per kind. The server only needs the placement, the optional
/// carried stack (item frame, weapons rack, food plate) and, for the training dummy, the NPC slot
/// it spawned. Kinds without a payload leave <see cref="Item"/> empty.
/// </remarks>
public readonly record struct WorldTileEntityState(
    short Id,
    WorldTileEntityKind Kind,
    WorldTileCoordinate Position,
    ItemStack Item = default,
    short NpcIndex = -1,
    bool SensorOn = false)
{
  /// <summary>Tile width of the multi-tile kinds, as Version4 declares per kind.</summary>
  public int TileWidth => Kind switch
  {
    WorldTileEntityKind.TeleportationPylon => 3,
    WorldTileEntityKind.DisplayDoll => 2,
    WorldTileEntityKind.DeadCellsDisplayJar => 2,
    _ => 1
  };

  /// <summary>Tile height of the multi-tile kinds, as Version4 declares per kind.</summary>
  public int TileHeight => Kind switch
  {
    WorldTileEntityKind.TeleportationPylon => 4,
    WorldTileEntityKind.DisplayDoll => 3,
    WorldTileEntityKind.DeadCellsDisplayJar => 3,
    _ => 1
  };

  public bool IsValid => Id >= 0 && Position.X >= 0 && Position.Y >= 0;
}
