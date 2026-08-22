using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class OasisPlantValidationQuery
{
  private const int TileFrameWidth = 18;

  public static OasisPlantValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    IReadOnlySet<ushort> conversionSandTileTypes,
    int x,
    int y,
    ushort plantType = 530)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(conversionSandTileTypes);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    int frameColumn = source.FrameX / TileFrameWidth;
    int styleBand = frameColumn / 3;
    int originX = x - frameColumn % 3;
    int originY = y - source.FrameY % 36 / TileFrameWidth;
    bool valid = true;
    for (int offsetX = 0; offsetX < 3; offsetX++)
    {
      for (int offsetY = 0; offsetY < 2; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        if (!snapshot.Metadata.IsInside(tileX, tileY))
        {
          valid = false;
          continue;
        }

        WorldTile tile = snapshot.GetTile(tileX, tileY);
        valid &= tile.IsActive && tile.Type == plantType &&
          tile.FrameX == checked((short)(styleBand * 54 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(offsetY * TileFrameWidth));
      }

      int supportY = originY + 2;
      WorldTile support = snapshot.Metadata.IsInside(originX + offsetX, supportY)
        ? snapshot.GetTile(originX + offsetX, supportY)
        : default;
      valid &= TileStateQuery.IsSolid(support, tileDefinitions) &&
        conversionSandTileTypes.Contains(support.Type);
    }

    return new OasisPlantValidationResult(valid, !valid, originX, originY, styleBand);
  }
}
