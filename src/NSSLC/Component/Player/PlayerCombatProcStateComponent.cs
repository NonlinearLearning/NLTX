namespace Terraria.Player;

/// <summary>
/// 保存玩家命中触发、吸血和装备效果冷却。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：lifeSteal（第 642 行）； ghostDmg（第 644 行）； eocDash（第 688 行）； eocHit（第 690 行）；
/// infernoCounter（第 736 行）； starCloakCooldown（第 739 行）； onHitDodge（第 798 行）； onHitRegen（第 800 行）；
/// onHitPetal（第 802 行）； onHitTitaniumStorm（第 804 行）； titaniumStormCooldown（第 806 行）；
/// hasTitaniumStormBuff（第 808 行）； petalTimer（第 810 行）； boneGloveTimer（第 814 行）；
/// phantomPhoneixCounter（第 816 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 74 行。</para>
/// </remarks>
public sealed class PlayerCombatProcStateComponent
{
  public float LifeSteal { get; set; } = 99999f;

  public float GhostDmg { get; set; }

  public int EocDash { get; set; }

  public int EocHit { get; set; } = -1;

  public int InfernoCounter { get; set; }

  public int StarCloakCooldown { get; set; }

  public bool OnHitDodge { get; set; }

  public bool OnHitRegen { get; set; }

  public bool OnHitPetal { get; set; }

  public bool OnHitTitaniumStorm { get; set; }

  public int TitaniumStormCooldown { get; set; }

  public bool HasTitaniumStormBuff { get; set; }

  public int PetalTimer { get; set; }

  public int BoneGloveTimer { get; set; }

  public int PhantomPhoneixCounter { get; set; }
}
