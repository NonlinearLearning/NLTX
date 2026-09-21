namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldLiquidSnapshot(
  int X,
  int Y,
  byte Amount,
  byte Type);
