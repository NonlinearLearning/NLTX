namespace Terraria.Player;

/// <summary>
/// 保存玩家折扣、幸运币和金币拾取相关能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：discountEquipped（第 2030 行）； discountAvailable（第 2032 行）； hasLuckyCoin（第 2034 行）；
/// goldRing（第 2038 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1099 行。</para>
/// </remarks>
public sealed class PlayerCommerceCapabilityComponent
{
  public bool DiscountEquipped { get; internal set; }

  public bool DiscountAvailable { get; internal set; }

  public bool HasLuckyCoin { get; internal set; }

  public bool GoldRing { get; internal set; }

  internal void ResetEffects()
  {
    DiscountEquipped = false;
    DiscountAvailable = false;
    HasLuckyCoin = false;
    GoldRing = false;
  }
}
