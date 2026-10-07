namespace Terraria.Npc;

public readonly record struct NpcDeathLifecycleResult(
  bool IsTerminal,
  bool Transitioned,
  bool AlreadyTerminal)
{
  public NpcDeathPhaseDecision? PhaseDecision { get; init; }

  public bool TerminalCommitPending { get; init; }
}
