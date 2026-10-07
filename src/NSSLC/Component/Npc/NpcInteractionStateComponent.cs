using System;
using System.Collections.Generic;

namespace Terraria.Npc;

// status: partial
// sourceMembers: playerInteraction, lastInteraction
// crossSubsystemOwner: interaction command owner integration-review
/// <summary>
/// 保存 NPC 与玩家的交互标记及最后交互者。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：playerInteraction（第 5973 行）； lastInteraction（第 5975 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P12-npc-combat-network-damage-component-design.md。</para>
/// <para>依据位置：第 13 行。</para>
/// </remarks>
public sealed class NpcInteractionStateComponent
{
  public NpcInteractionStateComponent(
    IReadOnlyList<bool>? playerInteraction = null,
    int lastInteraction = -1)
  {
    PlayerInteraction = playerInteraction is null
      ? ReadOnlyMemory<bool>.Empty
      : new ReadOnlyMemory<bool>(new List<bool>(playerInteraction).ToArray());
    LastInteraction = lastInteraction;
  }

  public ReadOnlyMemory<bool> PlayerInteraction { get; }

  public int LastInteraction { get; }
}
