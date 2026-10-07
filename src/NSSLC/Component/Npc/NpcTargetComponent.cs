using System;
using Terraria.Relationships;

namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 当前目标、旧目标索引和选择时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：target（第 6321 行）； oldTarget（第 6363 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-npc-and-town-simulation-component-design.md。</para>
/// <para>依据位置：第 332 行。</para>
/// </remarks>
public sealed class NpcTargetComponent
{
  public NpcTargetComponent(
    NpcTargetKind targetKind = NpcTargetKind.None,
    EntityReference? targetReference = null,
    int legacyTargetIndex = -1,
    int previousLegacyTargetIndex = -1)
  {
    if (legacyTargetIndex < -1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(legacyTargetIndex),
        "Legacy target index must be -1 or a non-negative index.");
    }

    if (previousLegacyTargetIndex < -1)
    {
      throw new ArgumentOutOfRangeException(
        nameof(previousLegacyTargetIndex),
        "Previous legacy target index must be -1 or a non-negative index.");
    }

    if (targetKind == NpcTargetKind.None && targetReference.HasValue)
    {
      throw new ArgumentException(
        "A target reference cannot be supplied when the target kind is None.",
        nameof(targetReference));
    }

    TargetKind = targetKind;
    TargetReference = targetReference;
    LegacyTargetIndex = legacyTargetIndex;
    PreviousLegacyTargetIndex = previousLegacyTargetIndex;
  }

  // Compatibility constructor for the existing selection timestamp API.
  public NpcTargetComponent(
    NpcTargetKind kind,
    EntityReference? targetEntity,
    long selectedAtTick)
    : this(kind, targetEntity, -1, -1)
  {
    SelectedAtTick = selectedAtTick;
  }

  public EntityReference? TargetReference { get; }

  public NpcTargetKind TargetKind { get; }

  public int LegacyTargetIndex { get; }

  public int PreviousLegacyTargetIndex { get; }

  public long SelectedAtTick { get; }

  // Compatibility aliases retained while callers move to the split API.
  public NpcTargetKind Kind => TargetKind;

  public EntityReference? TargetEntity => TargetReference;

  public bool HasTarget =>
    TargetKind != NpcTargetKind.None && TargetReference.HasValue;
}
