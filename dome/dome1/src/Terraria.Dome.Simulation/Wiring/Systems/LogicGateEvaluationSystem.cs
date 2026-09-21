using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class LogicGateEvaluationSystem
{
  public MechanismActivationCommand? Evaluate(
    LogicGateComponent gate,
    IReadOnlyDictionary<int, bool> inputs,
    long sequence)
  {
    ArgumentNullException.ThrowIfNull(inputs);
    if (sequence < 0 || gate.Inputs.Count == 0)
    {
      return null;
    }

    bool result = gate.IsAnd;
    for (int index = 0; index < gate.Inputs.Count; index++)
    {
      bool active = inputs.TryGetValue(gate.Inputs[index], out bool value) && value;
      result = gate.IsAnd ? result && active : result || active;
    }

    return result
      ? new MechanismActivationCommand(
        sequence,
        gate.OutputMechanismId,
        MechanismActivationKind.Activate,
        gate.MechanismId)
      : null;
  }
}
