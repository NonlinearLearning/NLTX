namespace Terraria.Player;

/// <summary>
/// 保存玩家各类弹药节省概率来源。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：chloroAmmoCost80（第 1391 行）； huntressAmmoCost90（第 1393 行）； ammoCost80（第 1395 行）；
/// ammoCost75（第 1397 行）； ammoBox（第 1411 行）； ammoPotion（第 1413 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 654 行。</para>
/// </remarks>
public sealed class PlayerAmmoCostPolicyComponent
{
  public bool ChloroAmmoCost80 { get; internal set; }

  public bool HuntressAmmoCost90 { get; internal set; }

  public bool AmmoCost80 { get; internal set; }

  public bool AmmoCost75 { get; internal set; }

  public bool AmmoBox { get; internal set; }

  public bool AmmoPotion { get; internal set; }

  internal void ResetEffects()
  {
    ChloroAmmoCost80 = false;
    HuntressAmmoCost90 = false;
    AmmoCost80 = false;
    AmmoCost75 = false;
    AmmoBox = false;
    AmmoPotion = false;
  }
}
