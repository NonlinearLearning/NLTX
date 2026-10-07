namespace Terraria.Player;

/// <summary>
/// 保存玩家潜水、游泳和药水站能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：accDivingHelm（第 2040 行）； accFlipper（第 2042 行）； deadCellsPotionStation（第 2044 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1101 行。</para>
/// </remarks>
public sealed class PlayerWaterAndUtilityCapabilityComponent
{
  public bool AccDivingHelm { get; internal set; }

  public bool AccFlipper { get; internal set; }

  public bool DeadCellsPotionStation { get; internal set; }

  internal void ResetEffects()
  {
    AccDivingHelm = false;
    AccFlipper = false;
    DeadCellsPotionStation = false;
  }
}
