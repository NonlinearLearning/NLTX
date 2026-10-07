using System;

namespace Terraria.Npc;

// status: proposed
// evidenceStatus: confirmed for Version4 localAI field; restore semantics partial
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存 NPC 不参与权威同步的局部 AI 槽位。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：localAI（第 6311 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 286 行。</para>
/// </remarks>
public sealed class NpcLocalBehaviorStateComponent
{
  public const int LocalAiSlotCount = 4;

  public NpcLocalBehaviorStateComponent(ReadOnlySpan<float> localAiSlots)
  {
    if (localAiSlots.Length != LocalAiSlotCount)
    {
      throw new ArgumentException(
        $"NPC local AI state must contain exactly {LocalAiSlotCount} slots.",
        nameof(localAiSlots));
    }

    LocalAiSlots = localAiSlots.ToArray();
  }

  public float[] LocalAiSlots { get; }
}
