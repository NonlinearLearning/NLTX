using Terraria.Dome.Simulation.Liquid.Components;

namespace Terraria.Dome.Simulation.Liquid.Definitions;

public readonly record struct LiquidMergeRule(
  LiquidType FirstType,
  LiquidType SecondType,
  LiquidType ResultType,
  ushort ResultTileType);
