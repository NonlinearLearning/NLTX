using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Wiring.Commands;

public readonly record struct DoorTransitionCommand(
  long Sequence,
  int DoorId,
  DoorTransition Transition,
  int SourceMechanismId);
