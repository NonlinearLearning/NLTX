namespace Terraria.Player;

/// <summary>
/// 保存玩家生命恢复速率、累积量和自然恢复进度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：lifeRegen（第 1369 行）； lifeRegenCount（第 1371 行）； lifeRegenTime（第 1373 行）。</para>
/// <para>重组说明：LifeRegenTime 是可被效果加速的自然恢复进度。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 563 行。</para>
/// </remarks>
public sealed class PlayerLifeRegenStateComponent
{
  public int LifeRegen { get; internal set; }

  public int LifeRegenCount { get; internal set; }

  public float LifeRegenTime { get; internal set; }

  internal void ResetEffects()
  {
    LifeRegen = 0;
  }

  internal void ResetForLifecycle()
  {
    LifeRegen = 0;
    LifeRegenCount = 0;
    LifeRegenTime = 0f;
  }
}
