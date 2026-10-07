namespace Terraria.Player;

/// <summary>
/// 保存玩家当前生效的钓鱼技能和饰品能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：fishingSkill（第 818 行）； cratePotion（第 820 行）； sonarPotion（第 822 行）； accFishingLine（第 824
/// 行）； accFishingBobber（第 826 行）； accTackleBox（第 828 行）； accLavaFishing（第 830 行）。
/// </para>
/// <para>重组说明：有效钓鱼等级和浮标覆盖由钓鱼流程重组为能力输入。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 97 行。</para>
/// </remarks>
public sealed class PlayerFishingCapabilityComponent
{
  public int BaseSkill { get; set; }

  public bool AllowsCrates { get; set; }

  public bool HasSonar { get; set; }

  public bool HasFishingLineProtection { get; set; }

  public bool HasBobberBonus { get; set; }

  public bool HasTackleBoxBonus { get; set; }

  public bool CanFishInLava { get; set; }

  public ContentId<ProjectileDefinition>? BobberOverrideType { get; set; }

  public int EffectiveFishingLevel { get; set; }
}
