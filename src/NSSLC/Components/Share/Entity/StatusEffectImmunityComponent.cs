using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>
/// 保存按状态效果定义编号索引的 Buff 与 Debuff 免疫表。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：buffImmune（第 1033 行）。</para>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：buffImmune（第 6071 行）。</para>
/// <para>重组说明：Revision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-06-version4-combat-and-status-component-design-report.md。</para>
/// <para>依据位置：第 243 行。</para>
/// <para>
/// 用途：添加状态效果前，外部流程按定义编号读取布尔表，判断该主体是否免疫该效果。
/// </para>
/// <para>
/// 当前实现：构造时复制输入表，以只读内存暴露结果；未提供输入时容量为零。Revision 单独保存，当前组件没有设置免疫或自动更新版本的方法。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C074，L2（通用可选玩法）。分级理由：有 Buff／Debuff 及免疫规则时才需要，普通扣血可直接结算。
/// </para>
/// </remarks>
public struct StatusEffectImmunityComponent
{
  public StatusEffectImmunityComponent(
    IReadOnlyList<bool>? immuneByDefinitionId = null,
    int revision = 0)
  {
    _immuneByDefinitionId = immuneByDefinitionId is null
      ? null
      : new List<bool>(immuneByDefinitionId).ToArray();
    Revision = revision;
  }

  private bool[]? _immuneByDefinitionId;

  /// <summary>
  /// 免疫状态的版本或实例管理值，默认零。
  /// </summary>
  public int Revision;

  /// <summary>
  /// 按效果定义编号排列的免疫标记只读视图；无表时为空。
  /// </summary>
  public ReadOnlyMemory<bool> ImmunityByDefinitionId =>
    new(_immuneByDefinitionId ?? Array.Empty<bool>());

  /// <summary>
  /// 当前免疫表能索引的定义编号数量。
  /// </summary>
  public int DefinitionCapacity => _immuneByDefinitionId?.Length ?? 0;
}
