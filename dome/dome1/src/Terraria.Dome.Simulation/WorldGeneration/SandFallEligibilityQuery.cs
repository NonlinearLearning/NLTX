using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SandFallEligibilityQuery
{
  private const ushort SandfallSupportExceptionType = 165;

  public static bool BlockBelowMakesSandConvertIntoHardenedSand(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y + 1))
    {
      return false;
    }

    WorldTile below = snapshot.GetTile(x, y + 1);
    return !below.IsActive || !TileStateQuery.IsSolidOrSloped(below, tileDefinitions);
  }

  public static bool BlockBelowMakesSandFall(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y + 1))
    {
      return false;
    }

    WorldTile below = snapshot.GetTile(x, y + 1);
    if (!below.IsActive || !TileStateQuery.IsSolidOrSloped(below, tileDefinitions))
    {
      return true;
    }

    WorldTile belowNext = snapshot.Metadata.IsInside(x, y + 2)
      ? snapshot.GetTile(x, y + 2)
      : default;
    return (!belowNext.IsActive &&
      (!below.IsActive || !TileStateQuery.IsSolidOrSloped(below, tileDefinitions))) ||
      (below.IsActive && below.Type == SandfallSupportExceptionType);
  }
}
