namespace Terraria.Player;

/// <summary>
/// 保存玩家魔力恢复速率和延迟加成。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：manaRegenBonus（第 674 行）； manaRegenDelayBonus（第 676 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 332 行。</para>
/// </remarks>
public sealed class PlayerManaRegenModifierComponent
{
  public int ManaRegenBonus { get; internal set; }

  public float ManaRegenDelayBonus { get; internal set; }

  internal void Reset()
  {
    ManaRegenBonus = 0;
    ManaRegenDelayBonus = 0f;
  }
}
