using System;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 与主实体或父 NPC 的关联。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：realLife（第 6039 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 429 行。</para>
/// </remarks>
public sealed class NpcParentRelationComponent
{
  public NpcParentRelationComponent(
    NpcInstanceId parentInstanceId,
    NpcSlot parentLegacySlot,
    long attachedAtTick)
  {
    if (attachedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(attachedAtTick),
        "Attached tick cannot be negative.");
    }

    ParentInstanceId = parentInstanceId;
    ParentLegacySlot = parentLegacySlot;
    AttachedAtTick = attachedAtTick;
  }

  public NpcInstanceId ParentInstanceId { get; }

  public NpcSlot ParentLegacySlot { get; }

  public long AttachedAtTick { get; }

  public bool HasLegacyParentSlot => ParentLegacySlot.IsAssigned;
}
