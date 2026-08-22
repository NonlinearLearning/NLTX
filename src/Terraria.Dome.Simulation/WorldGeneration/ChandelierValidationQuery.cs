using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ChandelierValidationQuery
{
  private const int ChandelierHeight = 3;
  private const int FrameWidth = 18;
  private const ushort SpecialChandelierType = 454;

  public static ChandelierValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort type)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    int width = type == SpecialChandelierType ? 4 : 3;
    WorldTile originTile = snapshot.GetTile(x, y);
    int originX = x - PositiveModulo(originTile.FrameX / FrameWidth, width);
    int originY = y - PositiveModulo(originTile.FrameY / FrameWidth, ChandelierHeight);
    int checkedTiles = 0;
    int invalidTiles = 0;
    for (int offsetX = 0; offsetX < width; offsetX++)
    {
      for (int offsetY = 0; offsetY < ChandelierHeight; offsetY++)
      {
        checkedTiles++;
        if (!snapshot.Metadata.IsInside(originX + offsetX, originY + offsetY))
        {
          invalidTiles++;
          continue;
        }

        WorldTile tile = snapshot.GetTile(originX + offsetX, originY + offsetY);
        if (!tile.IsActive || tile.Type != type)
        {
          invalidTiles++;
        }
      }
    }

    bool hasSolidSupport = snapshot.Metadata.IsInside(originX + 1, originY - 1) &&
      TileStateQuery.IsSolid(snapshot, tileDefinitions, originX + 1, originY - 1);
    if (!hasSolidSupport)
    {
      invalidTiles++;
    }

    bool isValid = invalidTiles == 0;
    return new ChandelierValidationResult(
      isValid,
      !isValid,
      originX,
      originY,
      width,
      ChandelierHeight,
      checkedTiles,
      invalidTiles,
      hasSolidSupport,
      true,
      true);
  }

  private static int PositiveModulo(int value, int modulus)
  {
    int remainder = value % modulus;
    return remainder < 0 ? remainder + modulus : remainder;
  }
}
