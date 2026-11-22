namespace Terraria.World;

/// <summary>
/// Port owned by the authoritative world implementation. It must not be implemented by the ECS
/// capability component; the adapter decides how tile rules, revisions, drops and broadcasts work.
/// Read snapshots belong to IWorldTileReader; this port only accepts authoritative writes.
/// </summary>
public interface IWorldInteractionWorld {
  WorldInteractionResult TryBreakTile(WorldTileCoordinate coordinate);

  WorldInteractionResult TryPlaceTile(WorldTileCoordinate coordinate, ushort tileType, byte style);
}
