namespace Terraria.Dome.Simulation.Commands;

public readonly record struct MechanismActivationCommand(
  long Sequence,
  int MechanismId,
  MechanismActivationKind Kind,
  int SourceId);
