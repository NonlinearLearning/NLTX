namespace Terraria.Npc;

public sealed class NpcTaskStateComponent
{
  public ulong TaskGeneration { get; private set; }

  public NpcTaskKind Kind { get; private set; }

  public NpcTaskPhase Phase { get; private set; } = NpcTaskPhase.Idle;

  public int Cursor { get; private set; }

  public NpcTaskFailureReason FailureReason { get; private set; }

  public NpcTaskEndReason LastEndReason { get; private set; }

  public ulong LastEndedTaskGeneration { get; private set; }

  public bool IsRunning => Phase == NpcTaskPhase.Running;

  internal void Start(NpcTaskKind kind, int cursor)
  {
    TaskGeneration = checked(TaskGeneration + 1);
    Kind = kind;
    Phase = NpcTaskPhase.Running;
    Cursor = cursor;
    FailureReason = NpcTaskFailureReason.None;
    LastEndReason = NpcTaskEndReason.None;
    LastEndedTaskGeneration = 0;
  }

  internal void Advance()
  {
    Cursor++;
  }

  internal void Complete()
  {
    Phase = NpcTaskPhase.Completed;
    FailureReason = NpcTaskFailureReason.None;
    LastEndReason = NpcTaskEndReason.None;
  }

  internal void Fail(NpcTaskFailureReason reason)
  {
    Phase = NpcTaskPhase.Failed;
    FailureReason = reason;
    LastEndReason = NpcTaskEndReason.None;
  }

  internal void Interrupt(NpcTaskFailureReason reason)
  {
    Phase = NpcTaskPhase.Interrupted;
    FailureReason = reason;
    LastEndReason = NpcTaskEndReason.None;
  }

  internal void End(NpcTaskEndReason reason)
  {
    Kind = NpcTaskKind.None;
    Phase = NpcTaskPhase.Idle;
    Cursor = 0;
    FailureReason = NpcTaskFailureReason.None;
    LastEndReason = reason;
    LastEndedTaskGeneration = TaskGeneration;
  }

  internal void Reset()
  {
    TaskGeneration = checked(TaskGeneration + 1);
    Kind = NpcTaskKind.None;
    Phase = NpcTaskPhase.Idle;
    Cursor = 0;
    FailureReason = NpcTaskFailureReason.None;
    LastEndReason = NpcTaskEndReason.None;
    LastEndedTaskGeneration = 0;
  }
}
