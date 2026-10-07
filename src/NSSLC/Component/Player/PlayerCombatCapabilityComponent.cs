namespace Terraria.Player;

/// <summary>
/// 保存玩家装备赋予的战斗能力和触发计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：spaceGun（第 1387 行）； chaosState（第 1415 行）； strongBees（第 1417 行）； sporeSac（第 1419 行）；
/// shinyStone（第 1421 行）； empressBrooch（第 1423 行）； volatileGelatin（第 1425 行）；
/// volatileGelatinCounter（第 1427 行）； hasMagiluminescence（第 1429 行）； shadowArmor（第 1431 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 606 行。</para>
/// </remarks>
public sealed class PlayerCombatCapabilityComponent
{
  public bool SpaceGun { get; internal set; }

  public bool ChaosState { get; internal set; }

  public bool StrongBees { get; internal set; }

  public bool SporeSac { get; internal set; }

  public bool ShinyStone { get; internal set; }

  public bool EmpressBrooch { get; internal set; }

  public bool VolatileGelatin { get; internal set; }

  public int VolatileGelatinCounter { get; internal set; }

  public bool HasMagiluminescence { get; internal set; }

  public bool ShadowArmor { get; internal set; }

  internal void ResetEffects()
  {
    SpaceGun = false;
    ChaosState = false;
    StrongBees = false;
    SporeSac = false;
    ShinyStone = false;
    EmpressBrooch = false;
    VolatileGelatin = false;
    VolatileGelatinCounter = 0;
    HasMagiluminescence = false;
    ShadowArmor = false;
  }
}
