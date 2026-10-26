namespace Terraria.World;

/// <summary>
/// Port owned by the authoritative world implementation. It must not be implemented by the ECS
/// capability component; the adapter decides how tile rules, revisions, drops and broadcasts work.
/// </summary>
public interface IWorldInteractionWorld {
  bool TryReadTile(WorldTileCoordinate coordinate, out WorldTileState tile);

  WorldInteractionResult TryBreakTile(WorldTileCoordinate coordinate);

  WorldInteractionResult TryPlaceTile(WorldTileCoordinate coordinate, ushort tileType, byte style);
}
