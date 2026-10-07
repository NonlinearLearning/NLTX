namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 可分步任务的代数、阶段、游标和结束原因。
/// </summary>
/// <remarks>
/// <para>拆分来源：由 NPC.AI 中可分步执行的目标搜索、住房和回家流程重组。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>重组说明：TaskGeneration、Phase、Cursor 和结束原因是任务执行与终止模型新增的状态。</para>
/// <para>拆分依据目录：docs/system-decomposition/。</para>
/// <para>拆分依据文件：2026-10-05-npc-ai-system-redesign.md。</para>
/// <para>依据位置：第 218 行。</para>
/// </remarks>
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
