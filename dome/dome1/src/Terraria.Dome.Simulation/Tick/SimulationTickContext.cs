using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Tick;

public sealed class SimulationTickContext
{
  private readonly List<SimulationTickPhase> _phases = new();

  internal SimulationTickContext()
  {
  }

  public IReadOnlyList<SimulationTickPhase> Phases => _phases.AsReadOnly();

  internal void Enter(SimulationTickPhase phase)
  {
    int phaseIndex = _phases.Count;
    if (phaseIndex >= SimulationTickSchedule.ActivePhases.Count ||
        SimulationTickSchedule.ActivePhases[phaseIndex] != phase)
    {
      throw new InvalidOperationException($"Unexpected tick phase: {phase}.");
    }

    _phases.Add(phase);
  }

  internal void CompleteNormally()
  {
    if (_phases.Count != SimulationTickSchedule.ActivePhases.Count)
    {
      throw new InvalidOperationException("The active tick did not reach every required phase.");
    }
  }

  internal void CompleteWhilePaused()
  {
    if (_phases.Count != 2 ||
        _phases[0] != SimulationTickPhase.BeginTick ||
        _phases[1] != SimulationTickPhase.ApplyWorldClock)
    {
      throw new InvalidOperationException("The paused tick reached a gameplay phase.");
    }

    _phases.Add(SimulationTickPhase.EndTick);
  }
}
