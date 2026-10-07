namespace Terraria.Player.Armor;

// status: implemented-isolated-core
// crossSubsystemOwner: integration-review for Buff, Combat, presentation, network, and persistence
/// <summary>
/// 保存甲虫套装的球数量、计时和攻防状态。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：beetleOrbs（第 565 行）； beetleCounter（第 567 行）； beetleCountdown（第 569 行）； beetleDefense（第
/// 571 行）； beetleOffense（第 573 行）； beetleBuff（第 575 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P07-player-mobility-component-design.md。</para>
/// <para>依据位置：第 175 行。</para>
/// </remarks>
public sealed class PlayerBeetleArmorStateComponent
{
  public int BeetleOrbCount { get; set; }

  public float BeetleCounter { get; set; }

  public int BeetleCountdown { get; set; }

  public bool HasDefenseSet { get; set; }

  public bool HasOffenseSet { get; set; }

  public bool BeetleBuffActive { get; set; }
}
