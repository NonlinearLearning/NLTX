namespace Terraria.Player;

/// <summary>
/// 保存星星斗篷及其不同变体的来源物品。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：starCloakItem（第 1815 行）； starCloakItem_manaCloakOverrideItem（第 1817 行）；
/// starCloakItem_starVeilOverrideItem（第 1819 行）； starCloakItem_beeCloakOverrideItem（第 1821 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 854 行。</para>
/// </remarks>
public sealed class PlayerStarCloakSourceRelationComponent
{
  public ItemEntityRef StarCloakItem { get; internal set; } = ItemEntityRef.None;

  public ItemEntityRef StarCloakItemManaCloakOverrideItem { get; internal set; } =
    ItemEntityRef.None;

  public ItemEntityRef StarCloakItemStarVeilOverrideItem { get; internal set; } =
    ItemEntityRef.None;

  public ItemEntityRef StarCloakItemBeeCloakOverrideItem { get; internal set; } =
    ItemEntityRef.None;

  internal void ResetEffects()
  {
    StarCloakItem = ItemEntityRef.None;
    StarCloakItemManaCloakOverrideItem = ItemEntityRef.None;
    StarCloakItemStarVeilOverrideItem = ItemEntityRef.None;
    StarCloakItemBeeCloakOverrideItem = ItemEntityRef.None;
  }
}
