namespace Terraria.Player.Progression;

/// <summary>
/// 保存玩家钓鱼技能、药水和饰品效果快照。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：fishingSkill（第 818 行）； cratePotion（第 820 行）； sonarPotion（第 822 行）； accFishingLine（第 824
/// 行）； accFishingBobber（第 826 行）； accTackleBox（第 828 行）； accLavaFishing（第 830 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 181 行。</para>
/// </remarks>
public sealed class PlayerFishingCapabilitySnapshotComponent
{
  public int FishingSkill { get; internal set; }

  public bool CratePotion { get; internal set; }

  public bool SonarPotion { get; internal set; }

  public bool AccFishingLine { get; internal set; }

  public bool AccFishingBobber { get; internal set; }

  public bool AccTackleBox { get; internal set; }

  public bool AccLavaFishing { get; internal set; }
}
