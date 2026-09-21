namespace Terraria.Dome.Simulation.Commands;

public readonly record struct LiquidTransferCommand(
  long Sequence,
  int SourceX,
  int SourceY,
  int TargetX,
  int TargetY,
  byte Amount,
  byte Type);
