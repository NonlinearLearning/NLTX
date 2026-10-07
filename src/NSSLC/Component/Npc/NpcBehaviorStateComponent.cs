namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 行为类别、旧 AI 风格和权威行为槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：ai（第 6309 行）； aiAction（第 6313 行）； aiStyle（第 6315 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-code-draft.md。</para>
/// <para>依据位置：第 74 行。</para>
/// </remarks>
public sealed class NpcBehaviorStateComponent
{
  public NpcBehaviorStateComponent(int behaviorKind, int legacyAiStyle, int action)
  {
    BehaviorKind = behaviorKind;
    LegacyAiStyle = legacyAiStyle;
    Action = action;
    AuthoritativeAiSlots = new float[4];
  }

  public int BehaviorKind { get; }

  public int LegacyAiStyle { get; }

  public int Action { get; set; }

  public float[] AuthoritativeAiSlots { get; }

  public long LastUpdatedTick { get; set; }

  public bool HasAuthoritativeSlots => AuthoritativeAiSlots.Length == 4;

  public void CommitAiStateSlots(in NpcAiStateComponent aiState)
  {
    if (!HasAuthoritativeSlots)
    {
      throw new InvalidOperationException("NPC AI state must contain exactly four slots.");
    }

    AuthoritativeAiSlots[0] = aiState.State0;
    AuthoritativeAiSlots[1] = aiState.State1;
    AuthoritativeAiSlots[2] = aiState.State2;
    AuthoritativeAiSlots[3] = aiState.State3;
  }
}
