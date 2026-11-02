namespace Terraria.World;

/// <summary>
/// One committed tile change that still needs to be projected to connected clients.
/// </summary>
public readonly record struct WorldMutation(
    WorldTileCoordinate Coordinate,
    WorldTileState Previous,
    WorldTileState Current,
    int DropItemType = 0,
    int DropStack = 1);
