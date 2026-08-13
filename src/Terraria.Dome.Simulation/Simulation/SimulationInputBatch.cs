using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation;

public sealed class SimulationInputBatch
{
  public SimulationInputBatch(params PlayerInput[] inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs);
    Inputs = inputs;
  }

  public IReadOnlyList<PlayerInput> Inputs { get; }
}
