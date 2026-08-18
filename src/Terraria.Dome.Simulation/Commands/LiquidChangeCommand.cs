namespace Terraria.Dome.Simulation.Commands;

public readonly record struct LiquidChangeCommand(
  long Sequence,
  int X,
  int Y,
  byte Amount,
  byte Type);
