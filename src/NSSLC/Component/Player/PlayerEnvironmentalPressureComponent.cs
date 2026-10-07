namespace Terraria.Player;

/// <summary>
/// 保存玩家风力推动和日照灼伤计数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：windPushed（第 1765 行）； sunScorchCounter（第 1779 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 756 行。</para>
/// </remarks>
public sealed class PlayerEnvironmentalPressureComponent
{
  public bool WindPushed { get; internal set; }

  public int SunScorchCounter { get; internal set; }

  internal void ResetEffects()
  {
    WindPushed = false;
  }

  internal void ResetForLifecycle()
  {
    WindPushed = false;
    SunScorchCounter = 0;
  }
}
