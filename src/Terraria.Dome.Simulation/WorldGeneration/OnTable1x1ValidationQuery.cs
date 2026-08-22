using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class OnTable1x1ValidationQuery
{
  public static OnTable1x1ValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    int type,
    bool hasTableAnchor,
    bool hasPlatformSideJoin)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (!snapshot.Metadata.IsInside(x, y + 1))
    {
      return new OnTable1x1ValidationResult(
        false,
        true,
        false,
        false,
        false,
        true,
        true);
    }

    WorldTile support = snapshot.GetTile(x, y + 1);
    if (IsTopSlope(support.Slope) || support.IsHalfBrick)
    {
      bool platformSupport = IsPlatformSideJoin(
        tileDefinitions,
        support,
        hasPlatformSideJoin);
      return new OnTable1x1ValidationResult(
        platformSupport,
        !platformSupport,
        platformSupport,
        false,
        false,
        true,
        true);
    }

    bool tableSupport = hasTableAnchor;
    bool type78BottomSlope = type == 78 &&
      TileStateQuery.IsSolidAllowingBottomSlope(support, tileDefinitions);
    bool solidSupport = TileStateQuery.IsSolidAllowingBottomSlope(support, tileDefinitions);
    bool supported = tableSupport || (type == 78 ? type78BottomSlope : solidSupport);
    return new OnTable1x1ValidationResult(
      supported,
      !supported,
      false,
      tableSupport,
      type78BottomSlope,
      false,
      true);
  }

  private static bool IsPlatformSideJoin(
    TileDefinitionRegistry tileDefinitions,
    WorldTile support,
    bool hasPlatformSideJoin)
  {
    if (!tileDefinitions.TryGet(support.Type, out TileDefinition definition) ||
        !definition.IsPlatform)
    {
      return false;
    }

    return hasPlatformSideJoin;
  }

  private static bool IsTopSlope(byte slope)
  {
    return slope is 1 or 2;
  }
}
