using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation;

public sealed class SimulationInputBatch
{
  public SimulationInputBatch(params PlayerInput[] inputs)
    : this(0, 0, inputs)
  {
  }

  public SimulationInputBatch(
    long inputSequenceStart,
    long inputSequenceEnd,
    params PlayerInput[] inputs)
  {
    ArgumentNullException.ThrowIfNull(inputs);
    if (inputSequenceStart < 0 || inputSequenceEnd < inputSequenceStart)
    {
      throw new ArgumentOutOfRangeException(nameof(inputSequenceEnd));
    }

    InputSequenceStart = inputSequenceStart;
    InputSequenceEnd = inputSequenceEnd;
    Inputs = inputs;
  }

  public long InputSequenceStart { get; }
  public long InputSequenceEnd { get; }
  public IReadOnlyList<PlayerInput> Inputs { get; }
}
