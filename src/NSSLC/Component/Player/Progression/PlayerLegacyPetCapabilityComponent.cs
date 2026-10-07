namespace Terraria.Player.Progression;

// status: implemented-core; local-core-verified; production-integration: unknown
// source-members: suspiciouslookingTentacle, crimsonHeart, lightOrb, blueFairy, redFairy,
// greenFairy, bunny, turtle, eater, penguin, HasGardenGnomeNearby, magicLantern, rabid,
// sunflower, wellFed, puppy, grinch, miniMinotaur, blackCat, spider, squashling
/// <summary>
/// 保存玩家旧宠物与相关辅助效果标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：suspiciouslookingTentacle（第 1483 行）； crimsonHeart（第 1485 行）； lightOrb（第 1487 行）；
/// blueFairy（第 1489 行）； redFairy（第 1491 行）； greenFairy（第 1493 行）； bunny（第 1495 行）； turtle（第 1497
/// 行）； eater（第 1499 行）； penguin（第 1501 行）； HasGardenGnomeNearby（第 1503 行）； magicLantern（第 1508
/// 行）； rabid（第 1510 行）； sunflower（第 1512 行）； wellFed（第 1514 行）； puppy（第 1516 行）； grinch（第 1518
/// 行）； miniMinotaur（第 1520 行）； blackCat（第 1554 行）； spider（第 1556 行）； squashling（第 1558 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 225 行。</para>
/// </remarks>
public sealed class PlayerLegacyPetCapabilityComponent
{
  public bool SuspiciousLookingTentacle { get; internal set; }

  public bool CrimsonHeart { get; internal set; }

  public bool LightOrb { get; internal set; }

  public bool BlueFairy { get; internal set; }

  public bool RedFairy { get; internal set; }

  public bool GreenFairy { get; internal set; }

  public bool Bunny { get; internal set; }

  public bool Turtle { get; internal set; }

  public bool Eater { get; internal set; }

  public bool Penguin { get; internal set; }

  public bool HasGardenGnomeNearby { get; internal set; }

  public bool MagicLantern { get; internal set; }

  public bool Rabid { get; internal set; }

  public bool Sunflower { get; internal set; }

  public bool WellFed { get; internal set; }

  public bool Puppy { get; internal set; }

  public bool Grinch { get; internal set; }

  public bool MiniMinotaur { get; internal set; }

  public bool BlackCat { get; internal set; }

  public bool Spider { get; internal set; }

  public bool Squashling { get; internal set; }
}
