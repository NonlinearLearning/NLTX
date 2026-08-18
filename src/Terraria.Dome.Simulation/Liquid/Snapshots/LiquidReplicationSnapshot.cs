namespace Terraria.Dome.Simulation.Liquid.Snapshots;

public readonly record struct LiquidReplicationSnapshot(
  int X,
  int Y,
  byte Amount,
  byte Type,
  long Revision);
