namespace Terraria.Player;

/// <summary>
/// 保存玩家召唤物伤害和击退修正。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：minionDamage（第 1873 行）； minionKB（第 1875 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 998 行。</para>
/// </remarks>
public sealed class PlayerSummonerDamageModifierComponent
{
  public float MinionDamage { get; internal set; } = 1f;

  public float MinionKb { get; internal set; }

  internal void ResetEffects()
  {
    MinionDamage = 1f;
    MinionKb = 0f;
  }
}
