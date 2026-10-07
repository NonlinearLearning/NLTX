using System;
using System.Collections.Generic;

namespace Terraria.Combat;

/// <summary>
/// 保存按 Buff 定义编号索引的 Buff／Debuff 免疫表。
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

  public int Revision;

  public ReadOnlyMemory<bool> ImmunityByDefinitionId =>
    new(_immuneByDefinitionId ?? Array.Empty<bool>());

  public int DefinitionCapacity => _immuneByDefinitionId?.Length ?? 0;
}
