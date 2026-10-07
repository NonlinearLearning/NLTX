using System;

namespace Terraria.Npc;

// status: partial
// evidenceStatus: confirmed for aiStyle/aiAction/ai[4]
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存 NPC AI 风格、动作和权威 AI 槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：ai（第 6309 行）； aiAction（第 6313 行）； aiStyle（第 6315 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 238 行。</para>
/// </remarks>
public sealed class NpcBehaviorComponent
{
  public const int AiSlotCount = 4;

  public NpcBehaviorComponent(
    int aiStyle,
    int action,
    ReadOnlySpan<float> aiSlots)
  {
    if (aiSlots.Length != AiSlotCount)
    {
      throw new ArgumentException(
        $"NPC AI state must contain exactly {AiSlotCount} slots.",
        nameof(aiSlots));
    }

    AiStyle = aiStyle;
    Action = action;
    AiSlots = aiSlots.ToArray();
  }

  public int AiStyle { get; private set; }

  public int Action { get; private set; }

  public float[] AiSlots { get; }

  internal void ApplyAiState(in NpcAiStateComponent aiState)
  {
    if (AiSlots.Length != AiSlotCount)
    {
      throw new InvalidOperationException(
        "NPC AI state must contain exactly four slots.");
    }

    AiSlots[0] = aiState.State0;
    AiSlots[1] = aiState.State1;
    AiSlots[2] = aiState.State2;
    AiSlots[3] = aiState.State3;
  }
}
