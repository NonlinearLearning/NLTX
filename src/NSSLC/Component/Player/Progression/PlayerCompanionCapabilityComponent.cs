namespace Terraria.Player.Progression;

// status: local-rebuild-core-verified; production-integration: unknown
// source-members: companionCube, babyFaceMonster, snowman, dino, skeletron, hornet,
// zephyrfish, tiki, parrot, truffle, sapling, cSapling, wisp, lizard
/// <summary>
/// 保存玩家常规宠物和伙伴的能力标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：companionCube（第 1670 行）； babyFaceMonster（第 1672 行）； snowman（第 1686 行）； dino（第 1690 行）；
/// skeletron（第 1692 行）； hornet（第 1694 行）； zephyrfish（第 1696 行）； tiki（第 1698 行）； parrot（第 1700 行）；
/// truffle（第 1702 行）； sapling（第 1704 行）； cSapling（第 1706 行）； wisp（第 1708 行）； lizard（第 1710 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P05-player-progression-pets-minions-component-design.md。
/// </para>
/// <para>依据位置：第 301 行。</para>
/// </remarks>
public sealed class PlayerCompanionCapabilityComponent
{
  public bool CompanionCube { get; internal set; }

  public bool BabyFaceMonster { get; internal set; }

  public bool Snowman { get; internal set; }

  public bool Dino { get; internal set; }

  public bool Skeletron { get; internal set; }

  public bool Hornet { get; internal set; }

  public bool Zephyrfish { get; internal set; }

  public bool Tiki { get; internal set; }

  public bool Parrot { get; internal set; }

  public bool Truffle { get; internal set; }

  public bool Sapling { get; internal set; }

  public bool CSapling { get; internal set; }

  public bool Wisp { get; internal set; }

  public bool Lizard { get; internal set; }
}
