namespace Terraria.Dome.Simulation.Wiring.Commands;

public readonly record struct TriggerExtractinatorCommand(
  int TargetX,
  int TargetY,
  long Sequence);
