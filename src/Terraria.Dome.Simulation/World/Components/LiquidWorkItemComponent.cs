namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct LiquidWorkItemComponent(
  int X,
  int Y,
  byte LiquidType,
  byte Amount,
  long Sequence,
  string Source = "worldgen.liquid");
