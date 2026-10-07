namespace Terraria.Player;

/// <summary>
/// 保存玩家生命、魔力恢复和受击免伤计时。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：manaRegenBonus（第 674 行）； manaRegenDelayBonus（第 676 行）； immuneNoBlink（第 970 行）；
/// immuneTime（第 972 行）； lifeRegen（第 1369 行）； lifeRegenCount（第 1371 行）； lifeRegenTime（第 1373 行）；
/// manaRegen（第 1375 行）； manaRegenCount（第 1377 行）； manaRegenDelay（第 1379 行）； hurtCooldowns（第 2475
/// 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>拆分依据文件：2026-09-05-version4-player-gameplay-component-code-draft.md。</para>
/// <para>依据位置：第 349 行。</para>
/// </remarks>
public sealed class PlayerRegenerationAndImmunityComponent
{
  public int LifeRegenRate { get; set; }

  public int LifeRegenAccumulator { get; set; }

  public float LifeRegenElapsed { get; set; }

  public int ManaRegenRate { get; set; }

  public int ManaRegenAccumulator { get; set; }

  public float ManaRegenDelay { get; set; }

  public int ManaRegenRateBonus { get; set; }

  public float ManaRegenDelayBonus { get; set; }

  public bool HasManaRegenBuff { get; set; }

  public int GeneralImmunityRemainingTicks { get; set; }

  // The Version4 immunity slot count remains unresolved.
  public int[] CooldownImmunityRemainingTicks { get; } = [];

  public bool SuppressImmunityBlink { get; set; }
}
