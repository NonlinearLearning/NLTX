using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class AlchemicalTileValidationQuery
{
  private const int TileFrameWidth = 18;
  private const byte LavaLiquidType = 1;

  public static AlchemicalTileValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile tile = snapshot.GetTile(x, y);
    int style = Math.Max(0, tile.FrameX / TileFrameWidth);
    WorldTile support = snapshot.Metadata.IsInside(x, y + 1)
      ? snapshot.GetTile(x, y + 1)
      : default;
    bool hasSupportedBase = support.IsActive &&
      IsSupportedType(style, support.Type) &&
      !support.IsHalfBrick;
    bool hasLavaContact = tile.LiquidAmount > 0 && tile.LiquidType == LavaLiquidType;
    bool valid = hasSupportedBase && !hasLavaContact;
    return new AlchemicalTileValidationResult(
      valid,
      !valid,
      style,
      hasSupportedBase,
      hasLavaContact);
  }

  private static bool IsSupportedType(int style, ushort type)
  {
    return style switch
    {
      0 => type is 109 or 2 or 477 or 492 or 78 or 380,
      1 => type is 60 or 78 or 380,
      2 => type is 0 or 59 or 78 or 380,
      3 => type is 661 or 662 or 199 or 203 or 23 or 25 or 78 or 380,
      4 => type is 53 or 78 or 380 or 116,
      5 => type is 57 or 633 or 78 or 380,
      6 => type is 78 or 380 or 147 or 161 or 163 or 164 or 200,
      _ => false
    };
  }
}
