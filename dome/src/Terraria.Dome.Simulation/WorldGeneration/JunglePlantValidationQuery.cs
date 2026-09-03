using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class JunglePlantValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort LargePlantTileType = 236;
  private const ushort MushroomPlantTileType = 238;
  private const ushort SandPlantTileType = 702;

  public static JunglePlantValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort tileType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    bool largeVariant = source.FrameY >= 36 || tileType is
      LargePlantTileType or MushroomPlantTileType or SandPlantTileType;
    int width = largeVariant ? 2 : 3;
    int height = 2;
    int frameColumn = source.FrameX / TileFrameWidth;
    int styleBand = frameColumn / width;
    int originX = x - frameColumn % width;
    int originY = y - source.FrameY % 36 / TileFrameWidth;
    bool valid = true;
    for (int offsetX = 0; offsetX < width; offsetX++)
    {
      for (int offsetY = 0; offsetY < height; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        if (!snapshot.Metadata.IsInside(tileX, tileY))
        {
          valid = false;
          continue;
        }

        WorldTile tile = snapshot.GetTile(tileX, tileY);
        int frameY = tileType is LargePlantTileType or MushroomPlantTileType or SandPlantTileType
          ? offsetY * TileFrameWidth
          : offsetY * TileFrameWidth;
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked(
            (short)(styleBand * width * TileFrameWidth + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)frameY);
      }

      int supportY = originY + height;
      WorldTile support = snapshot.Metadata.IsInside(originX + offsetX, supportY)
        ? snapshot.GetTile(originX + offsetX, supportY)
        : default;
      valid &= tileType == SandPlantTileType
        ? TileStateQuery.IsSolidAllowingBottomSlope(support, tileDefinitions)
        : TileStateQuery.IsSolid(support, tileDefinitions) && support.Type == 60;
    }

    return new JunglePlantValidationResult(
      valid,
      !valid,
      originX,
      originY,
      width,
      height,
      tileType == SandPlantTileType);
  }
}
