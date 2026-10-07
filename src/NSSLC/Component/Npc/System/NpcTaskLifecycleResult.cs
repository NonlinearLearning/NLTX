namespace Terraria.Npc;

public readonly record struct NpcTaskLifecycleResult(
  bool Accepted,
  bool Changed,
  NpcTaskKind Kind,
  NpcTaskPhase Phase,
  int Cursor,
  NpcTaskFailureReason FailureReason)
{
  public ulong TaskGeneration { get; init; }

  public NpcTaskEndReason EndReason { get; init; }
}
