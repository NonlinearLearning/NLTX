using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Chest.Systems;

public static class LegacyChestOriginQuery
{
  private const ushort BasicChestTileType = 21;
  private const ushort DresserTileType = 88;
  private const ushort SecondBasicChestTileType = 467;
  private const short TileFrameSize = 18;

  public static bool TryGetOrigin(
    WorldTile tile,
    int tileX,
    int tileY,
    out int originX,
    out int originY)
  {
    originX = default;
    originY = default;
    if (!tile.IsActive || tile.FrameX < 0 || tile.FrameY < 0 ||
        tile.FrameX % TileFrameSize != 0 || tile.FrameY % TileFrameSize != 0)
    {
      return false;
    }

    int frameX = tile.FrameX / TileFrameSize;
    int frameY = tile.FrameY / TileFrameSize;
    switch (tile.Type)
    {
      case BasicChestTileType:
      case SecondBasicChestTileType:
        originX = tileX - frameX % 2;
        originY = tileY - frameY;
        return true;
      case DresserTileType:
        originX = tileX - frameX % 3;
        originY = tileY - frameY;
        return true;
      default:
        return false;
    }
  }
}
