namespace Terraria.Player;

/// <summary>
/// 保存玩家通用受击免伤标记、剩余时间和闪烁行为。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：immune（第 968 行）； immuneNoBlink（第 970 行）； immuneTime（第 972 行）； _timeSinceLastImmuneGet（第
/// 980 行）； _immuneStrikes（第 982 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 81 行。</para>
/// </remarks>
public sealed class PlayerHitFrameImmunityComponent
{
  public bool Immune { get; internal set; }

  public bool ImmuneNoBlink { get; internal set; }

  public int ImmuneTime { get; internal set; }

  public int TimeSinceLastImmuneGet { get; internal set; }

  public int ImmuneStrikes { get; internal set; }

  internal void ResetForLifecycle()
  {
    Immune = false;
    ImmuneNoBlink = false;
    ImmuneTime = 0;
    TimeSinceLastImmuneGet = 0;
    ImmuneStrikes = 0;
  }
}
