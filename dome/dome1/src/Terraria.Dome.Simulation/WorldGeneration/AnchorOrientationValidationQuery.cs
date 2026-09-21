using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class AnchorOrientationValidationQuery
{
  public static AnchorOrientationValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    int style,
    ushort wallType,
    bool switchToWallIfInvalid = false)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    int originalStyle = style;
    if (IsStyleSupported(snapshot, tileDefinitions, x, y, style, wallType))
    {
      return CreateResult(originalStyle, style, false, true);
    }

    style = FindSupportedStyle(snapshot, tileDefinitions, x, y, wallType);
    if (style < 0 && switchToWallIfInvalid && wallType > 0)
    {
      style = 4;
    }

    bool valid = style >= 0;
    return CreateResult(originalStyle, style, style == 4 && wallType > 0, valid);
  }

  private static AnchorOrientationValidationResult CreateResult(
    int originalStyle,
    int suggestedStyle,
    bool usedWallFallback,
    bool valid)
  {
    return new AnchorOrientationValidationResult(
      valid,
      !valid,
      originalStyle,
      suggestedStyle,
      usedWallFallback,
      true);
  }

  private static int FindSupportedStyle(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort wallType)
  {
    if (IsStyleSupported(snapshot, tileDefinitions, x, y, 0, wallType))
    {
      return 0;
    }

    if (IsStyleSupported(snapshot, tileDefinitions, x, y, 1, wallType))
    {
      return 1;
    }

    if (IsStyleSupported(snapshot, tileDefinitions, x, y, 2, wallType))
    {
      return 2;
    }

    if (IsStyleSupported(snapshot, tileDefinitions, x, y, 3, wallType))
    {
      return 3;
    }

    return wallType > 0 ? 4 : -1;
  }

  private static bool IsStyleSupported(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    int style,
    ushort wallType)
  {
    return style switch
    {
      0 => IsInside(snapshot, x, y + 1) &&
        TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, y + 1),
      1 => IsInside(snapshot, x, y - 1) &&
        TileStateQuery.IsSolidAllowingTopSlope(snapshot, tileDefinitions, x, y - 1),
      2 => IsInside(snapshot, x - 1, y) &&
        TileStateQuery.IsSolidAllowingLeftSlope(snapshot, tileDefinitions, x - 1, y),
      3 => IsInside(snapshot, x + 1, y) &&
        TileStateQuery.IsSolidAllowingRightSlope(snapshot, tileDefinitions, x + 1, y),
      4 => wallType > 0,
      _ => false
    };
  }

  private static bool IsInside(WorldGridSnapshot snapshot, int x, int y)
  {
    return snapshot.Metadata.IsInside(x, y);
  }
}
