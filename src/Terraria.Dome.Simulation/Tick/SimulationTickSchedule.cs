using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;

namespace Terraria.Dome.Simulation.Tick;

public static class SimulationTickSchedule
{
  private static readonly IReadOnlyList<SimulationTickPhase> _activePhases =
    Array.AsReadOnly(new[]
    {
      SimulationTickPhase.BeginTick,
      SimulationTickPhase.ApplyWorldClock,
      SimulationTickPhase.ApplyPlayerInputs,
      SimulationTickPhase.ApplyPlayerControl,
      SimulationTickPhase.ResolveTileCollision,
      SimulationTickPhase.SelectNpcTargets,
      SimulationTickPhase.ApplyNpcAi,
      SimulationTickPhase.MoveEntities,
      SimulationTickPhase.AdvanceProjectiles,
      SimulationTickPhase.ResolveCombat,
      SimulationTickPhase.CommitDomainCommands,
      SimulationTickPhase.PublishSnapshot,
      SimulationTickPhase.EndTick
    });

  public static IReadOnlyList<SimulationTickPhase> ActivePhases => _activePhases;

  internal static SimulationTickContext Begin()
  {
    return new SimulationTickContext();
  }

  internal static int CompareMechanismActivationCommands(
    MechanismActivationCommand first,
    MechanismActivationCommand second)
  {
    int sequence = first.Sequence.CompareTo(second.Sequence);
    if (sequence != 0)
    {
      return sequence;
    }

    int mechanism = first.MechanismId.CompareTo(second.MechanismId);
    return mechanism != 0 ? mechanism : first.SourceId.CompareTo(second.SourceId);
  }
}
