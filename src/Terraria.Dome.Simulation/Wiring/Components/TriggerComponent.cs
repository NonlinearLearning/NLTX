namespace Terraria.Dome.Simulation.Wiring.Components;

public readonly record struct TriggerComponent(
  int TriggerId,
  int TargetMechanismId,
  bool OneShot);
