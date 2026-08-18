namespace Terraria.Dome.Simulation.Liquid.Components;

public readonly record struct LiquidMergeComponent(
  int X,
  int Y,
  LiquidType FirstType,
  LiquidType SecondType,
  ushort ResultTileType,
  LiquidType ResultType,
  int NeighborX = -1,
  int NeighborY = -1);
