using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Definitions;

public readonly record struct MechanismDefinition(
  MechanismType Type,
  int CooldownTicks,
  bool ProducesEvent);
