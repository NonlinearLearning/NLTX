namespace Terraria.Player;

/// <summary>
/// 保存玩家魔力恢复速率、累积量、延迟和恢复 Buff。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：manaRegen（第 1375 行）； manaRegenCount（第 1377 行）； manaRegenDelay（第 1379 行）； manaRegenBuff（第
/// 1381 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 564 行。</para>
/// </remarks>
public sealed class PlayerManaRegenStateComponent
{
  public int ManaRegen { get; internal set; }

  public int ManaRegenCount { get; internal set; }

  public float ManaRegenDelay { get; internal set; }

  public bool ManaRegenBuff { get; internal set; }

  internal void ResetEffects()
  {
    ManaRegen = 0;
    ManaRegenBuff = false;
  }

  internal void ResetForLifecycle()
  {
    ManaRegen = 0;
    ManaRegenCount = 0;
    ManaRegenDelay = 0f;
    ManaRegenBuff = false;
  }
}
