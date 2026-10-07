namespace Terraria.Player;

/// <summary>
/// 保存玩家滴水、滴史莱姆等表面效果。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：dripping（第 1743 行）； drippingSlime（第 1745 行）； drippingSparkleSlime（第 1747 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 705 行。</para>
/// </remarks>
public sealed class PlayerSurfaceElementStatusComponent
{
  public bool Dripping { get; internal set; }

  public bool DrippingSlime { get; internal set; }

  public bool DrippingSparkleSlime { get; internal set; }

  internal void ResetEffects()
  {
    Dripping = false;
    DrippingSlime = false;
    DrippingSparkleSlime = false;
  }
}
