using System;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Liquid.Definitions;
using Terraria.Dome.Simulation.WorldModel;
using SimulationLiquidMerge = Terraria.Dome.Simulation.Liquid.Components.LiquidMergeComponent;

namespace Terraria.Dome.Simulation.Liquid.Systems;

public sealed class LiquidMergeSystem
{
  private const byte MinimumMergeAmount = 24;
  private readonly LiquidRuleRegistry _rules;

  public LiquidMergeSystem(LiquidRuleRegistry? rules = null)
  {
    _rules = rules ?? LiquidRuleRegistry.CreateDefault();
  }

  public bool TryEvaluate(WorldGrid world, int x, int y, out SimulationLiquidMerge merge)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!world.Contains(x, y))
    {
      merge = default;
      return false;
    }

    WorldTile tile = world.GetTile(x, y);
    if (tile.LiquidAmount == 0)
    {
      merge = default;
      return false;
    }

    (int X, int Y)[] neighbors =
    [
      (x - 1, y),
      (x + 1, y),
      (x, y - 1),
      (x, y + 1)
    ];
    for (int index = 0; index < neighbors.Length; index++)
    {
      (int neighborX, int neighborY) = neighbors[index];
      if (!world.Contains(neighborX, neighborY))
      {
        continue;
      }

      WorldTile neighbor = world.GetTile(neighborX, neighborY);
      if (neighbor.LiquidAmount < MinimumMergeAmount ||
          neighbor.LiquidType == tile.LiquidType ||
          !_rules.TryGetMerge(
            (LiquidType)tile.LiquidType,
            (LiquidType)neighbor.LiquidType,
            out LiquidMergeRule rule))
      {
        continue;
      }

      merge = new SimulationLiquidMerge(
        x,
        y,
        rule.FirstType,
        rule.SecondType,
        rule.ResultTileType,
        rule.ResultType,
        neighborX,
        neighborY);
      return true;
    }

    merge = default;
    return false;
  }
}
