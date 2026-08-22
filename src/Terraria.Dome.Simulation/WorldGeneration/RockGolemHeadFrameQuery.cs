using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RockGolemHeadFrameQuery
{
  private const ushort RockGolemHeadTileType = 579;

  public static RockGolemHeadFrameResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    if (!source.IsActive || source.Type != RockGolemHeadTileType)
    {
      return new RockGolemHeadFrameResult(false, false);
    }

    bool supported = TileStateQuery.IsSolidAllowingBottomSlope(
      snapshot,
      tileDefinitions,
      x,
      y + 1);
    return supported
      ? new RockGolemHeadFrameResult(true, false)
      : new RockGolemHeadFrameResult(false, true);
  }
}
