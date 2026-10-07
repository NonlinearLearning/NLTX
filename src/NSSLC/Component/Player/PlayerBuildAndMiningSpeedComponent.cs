namespace Terraria.Player;

/// <summary>
/// 保存玩家挖掘、放墙和放置速度修正。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：pickSpeed（第 1885 行）； wallSpeed（第 1887 行）； tileSpeed（第 1889 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 1050 行。</para>
/// </remarks>
public sealed class PlayerBuildAndMiningSpeedComponent
{
  public float PickSpeed { get; internal set; } = 1f;

  public float WallSpeed { get; internal set; } = 1f;

  public float TileSpeed { get; internal set; } = 1f;

  internal void ResetEffects()
  {
    PickSpeed = 1f;
    WallSpeed = 1f;
    TileSpeed = 1f;
  }
}
