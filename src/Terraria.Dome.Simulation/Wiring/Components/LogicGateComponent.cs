using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Wiring.Components;

public readonly record struct LogicGateComponent(
  int MechanismId,
  IReadOnlyList<int> Inputs,
  bool IsAnd,
  int OutputMechanismId);
