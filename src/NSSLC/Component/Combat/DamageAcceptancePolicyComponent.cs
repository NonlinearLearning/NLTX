namespace Terraria.Combat;

/// <summary>
/// 保存 NPC 对全部伤害、敌对伤害和陷阱伤害的接受策略及不死标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>
/// 主要源成员：immortal（第 6125 行）； dontTakeDamageFromHostiles（第 6143 行）； trapImmune（第 6335 行）；
/// dontTakeDamage（第 6387 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-06-version4-combat-and-status-component-design-report.md。</para>
/// <para>依据位置：第 152 行。</para>
/// </remarks>
public struct DamageAcceptancePolicyComponent
{
  public DamageAcceptancePolicyComponent(
    bool rejectAllDamage,
    bool rejectHostileDamage,
    bool rejectTrapDamage,
    bool isImmortal)
  {
    RejectAllDamage = rejectAllDamage;
    RejectHostileDamage = rejectHostileDamage;
    RejectTrapDamage = rejectTrapDamage;
    IsImmortal = isImmortal;
  }

  public bool RejectAllDamage;
  public bool RejectHostileDamage;
  public bool RejectTrapDamage;
  public bool IsImmortal;

  public bool CanAcceptAnyDamage => !RejectAllDamage;
}
