using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>
/// 保存 Buff／Debuff 槽位中的效果编号和剩余持续时间。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：buffType（第 1029 行）； buffTime（第 1031 行）。</para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：buffType（第 6067 行）； buffTime（第 6069 行）。</para>
/// <para>重组说明：原来的平行数组按槽位重组为 StatusEffectSlot；Revision 是槽位变更版本。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 56 行。</para>
/// </remarks>
public struct StatusEffectSlotsComponent
{
  public StatusEffectSlotsComponent(
    IReadOnlyList<StatusEffectSlot>? slots = null,
    int revision = 0)
  {
    _slots = slots is null
      ? null
      : new List<StatusEffectSlot>(slots).ToArray();
    Revision = revision;
  }

  private StatusEffectSlot[]? _slots;

  public int Revision;

  public ReadOnlyMemory<StatusEffectSlot> Slots =>
    new(_slots ?? Array.Empty<StatusEffectSlot>());

  public int Capacity => _slots?.Length ?? 0;
}
