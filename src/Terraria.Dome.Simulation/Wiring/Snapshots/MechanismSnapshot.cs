using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Snapshots;

public readonly record struct MechanismSnapshot(
  int MechanismId,
  MechanismType Type,
  bool IsActive,
  long LastActivationSequence);
