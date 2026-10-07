namespace Terraria.Player;

/// <summary>
/// 保存手套击退、自动挥舞及近战尺寸加成。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：kbGlove（第 1805 行）； autoReuseGlove（第 1807 行）； meleeScaleGlove（第 1809 行）； kbBuff（第 1811
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 852 行。</para>
/// </remarks>
public sealed class PlayerAccessoryMeleeCapabilityComponent
{
  public bool KbGlove { get; internal set; }

  public bool AutoReuseGlove { get; internal set; }

  public bool MeleeScaleGlove { get; internal set; }

  public bool KbBuff { get; internal set; }

  internal void ResetEffects()
  {
    KbGlove = false;
    AutoReuseGlove = false;
    MeleeScaleGlove = false;
    KbBuff = false;
  }
}
