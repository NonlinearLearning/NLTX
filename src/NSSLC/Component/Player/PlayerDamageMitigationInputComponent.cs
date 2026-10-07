namespace Terraria.Player;

/// <summary>
/// 保存玩家伤害结算的减伤比例输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：endurance（第 767 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 435 行。</para>
/// </remarks>
public sealed class PlayerDamageMitigationInputComponent
{
  public float Endurance { get; internal set; }

  internal void ResetEffects()
  {
    Endurance = 0f;
  }
}
