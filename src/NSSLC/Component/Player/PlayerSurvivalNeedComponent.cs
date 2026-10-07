namespace Terraria.Player;

/// <summary>
/// 保存玩家禁用物品、饥饿、饱食和食物状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：noItems（第 1755 行）； hungry（第 1759 行）； starving（第 1761 行）； heartyMeal（第 1763 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 754 行。</para>
/// </remarks>
public sealed class PlayerSurvivalNeedComponent
{
  public bool NoItems { get; internal set; }

  public bool Hungry { get; internal set; }

  public bool Starving { get; internal set; }

  public bool HeartyMeal { get; internal set; }

  internal void ResetEffects()
  {
    NoItems = false;
    Hungry = false;
    Starving = false;
    HeartyMeal = false;
  }
}
