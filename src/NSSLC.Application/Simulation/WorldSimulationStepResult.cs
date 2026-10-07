using System.Collections.ObjectModel;

namespace Terraria.NonAuthoritative.Simulation;

public sealed class WorldSimulationStepResult
{
  internal WorldSimulationStepResult(
    bool tickCommitted,
    long tickNumber,
    int commandsApplied,
    IReadOnlyList<WorldSimulationPhase> phasesExecuted,
    WorldSimulationKernelStatus status)
  {
    TickCommitted = tickCommitted;
    TickNumber = tickNumber;
    CommandsApplied = commandsApplied;
    PhasesExecuted = new ReadOnlyCollection<WorldSimulationPhase>(phasesExecuted.ToArray());
    Status = status;
  }

  public bool TickCommitted { get; }

  public long TickNumber { get; }

  public int CommandsApplied { get; }

  public IReadOnlyList<WorldSimulationPhase> PhasesExecuted { get; }

  public WorldSimulationKernelStatus Status { get; }
}
