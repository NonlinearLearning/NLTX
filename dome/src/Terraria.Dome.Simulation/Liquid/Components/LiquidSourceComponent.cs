namespace Terraria.Dome.Simulation.Liquid.Components;

public readonly record struct LiquidSourceComponent(
  int X,
  int Y,
  byte Amount,
  LiquidType Type,
  long Sequence);
