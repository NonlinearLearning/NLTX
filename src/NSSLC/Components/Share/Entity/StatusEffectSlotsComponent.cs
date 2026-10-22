using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>
/// 保存 Buff 与 Debuff 槽位中的效果编号、剩余时间和变更版本。
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
/// <para>
/// 用途：把原版效果编号与持续时间的平行数组合并成 StatusEffectSlot 槽位，供状态系统按槽位读取。
/// </para>
/// <para>
/// 当前实现：构造时复制槽位列表，以只读内存暴露；无输入时容量为零。效果添加、消耗时间、清空及版本更新由外部流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C075，L2（通用可选玩法）。分级理由：有 Buff／Debuff 及免疫规则时才需要，普通扣血可直接结算。
/// </para>
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

  /// <summary>
  /// 槽位状态变更版本，当前组件不会自动递增。
  /// </summary>
  public int Revision;

  /// <summary>
  /// 构造时复制的效果槽位只读视图。
  /// </summary>
  public ReadOnlyMemory<StatusEffectSlot> Slots =>
    new(_slots ?? Array.Empty<StatusEffectSlot>());

  /// <summary>
  /// 当前保存的槽位数量。
  /// </summary>
  public int Capacity => _slots?.Length ?? 0;
}
