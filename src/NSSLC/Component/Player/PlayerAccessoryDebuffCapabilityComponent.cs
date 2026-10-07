namespace Terraria.Player;

/// <summary>
/// 保存饰品及战斗来源的减益和反击增益标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：vortexDebuff（第 1833 行）； trapDebuffSource（第 1835 行）； witheredArmor（第 1837 行）；
/// witheredWeapon（第 1839 行）； slowOgreSpit（第 1841 行）； parryDamageBuff（第 1843 行）； ballistaPanic（第
/// 1845 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 948 行。</para>
/// </remarks>
public sealed class PlayerAccessoryDebuffCapabilityComponent
{
  public bool VortexDebuff { get; internal set; }

  public bool TrapDebuffSource { get; internal set; }

  public bool WitheredArmor { get; internal set; }

  public bool WitheredWeapon { get; internal set; }

  public bool SlowOgreSpit { get; internal set; }

  public bool ParryDamageBuff { get; internal set; }

  public bool BallistaPanic { get; internal set; }

  internal void ResetEffects()
  {
    VortexDebuff = false;
    TrapDebuffSource = false;
    WitheredArmor = false;
    WitheredWeapon = false;
    SlowOgreSpit = false;
    ParryDamageBuff = false;
    BallistaPanic = false;
  }
}
