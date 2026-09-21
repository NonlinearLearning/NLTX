using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Tick;

public sealed record SimulationTickTrace
{
  public SimulationTickTrace(
    long tickNumber,
    long inputSequenceStart,
    long inputSequenceEnd,
    int commandCount,
    int eventCount,
    IReadOnlyList<SimulationTickPhase> phases)
  {
    if (tickNumber < 0 || inputSequenceStart < 0 || inputSequenceEnd < inputSequenceStart)
    {
      throw new ArgumentOutOfRangeException(nameof(tickNumber));
    }

    if (commandCount < 0 || eventCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(commandCount));
    }

    ArgumentNullException.ThrowIfNull(phases);
    TickNumber = tickNumber;
    InputSequenceStart = inputSequenceStart;
    InputSequenceEnd = inputSequenceEnd;
    CommandCount = commandCount;
    EventCount = eventCount;
    Phases = new List<SimulationTickPhase>(phases).AsReadOnly();
  }

  public long TickNumber { get; }
  public long InputSequenceStart { get; }
  public long InputSequenceEnd { get; }
  public int CommandCount { get; }
  public int EventCount { get; }
  public IReadOnlyList<SimulationTickPhase> Phases { get; }
}
