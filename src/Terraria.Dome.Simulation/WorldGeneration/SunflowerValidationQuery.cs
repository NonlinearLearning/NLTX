using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SunflowerValidationQuery
{
  private const int FootprintHeight = 4;
  private const int FootprintWidth = 2;
  private const int FrameWidth = 18;

  public static SunflowerValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    int type = 27)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile originTile = snapshot.GetTile(x, y);
    int originX = x - PositiveModulo(originTile.FrameX / FrameWidth, FootprintWidth);
    int originY = y - originTile.FrameY / FrameWidth;
    int checkedTiles = 0;
    int invalidTiles = 0;
    int validGroundTiles = 0;
    for (int offsetX = 0; offsetX < FootprintWidth; offsetX++)
    {
      for (int offsetY = 0; offsetY < FootprintHeight; offsetY++)
      {
        checkedTiles++;
        if (!IsExpectedFootprintTile(
              snapshot,
              originX + offsetX,
              originY + offsetY,
              offsetX,
              offsetY,
              type))
        {
          invalidTiles++;
        }
      }

      if (HasAllowedGround(
            snapshot,
            tileDefinitions,
            originX + offsetX,
            originY + FootprintHeight))
      {
        validGroundTiles++;
      }
      else
      {
        invalidTiles++;
      }
    }

    bool hasAllowedGround = validGroundTiles == FootprintWidth;
    bool isValid = invalidTiles == 0;
    return new SunflowerValidationResult(
      isValid,
      !isValid,
      originX,
      originY,
      checkedTiles,
      invalidTiles,
      hasAllowedGround,
      true);
  }

  private static bool IsExpectedFootprintTile(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    int offsetX,
    int offsetY,
    int type)
  {
    if (!snapshot.Metadata.IsInside(x, y))
    {
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    int frameColumn = PositiveModulo(tile.FrameX / FrameWidth, FootprintWidth);
    return tile.IsActive &&
      tile.Type == type &&
      frameColumn == offsetX &&
      tile.FrameY == offsetY * FrameWidth;
  }

  private static bool HasAllowedGround(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    if (!snapshot.Metadata.IsInside(x, y))
    {
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    bool allowedType = tile.Type is 2 or 477 or 492 or 109 or 60 or 633;
    return allowedType && TileStateQuery.IsSolid(snapshot, tileDefinitions, x, y);
  }

  private static int PositiveModulo(int value, int modulus)
  {
    int remainder = value % modulus;
    return remainder < 0 ? remainder + modulus : remainder;
  }
}
