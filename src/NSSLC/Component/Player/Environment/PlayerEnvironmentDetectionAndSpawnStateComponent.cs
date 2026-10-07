namespace Terraria.Player.Environment;

// status: implemented
// componentId: PLAYER.COMP.ENVIRONMENT_DETECTION_AND_SPAWN_STATE
// source-members: P08-1261..P08-1265, P08-1269..P08-1274, P08-1282..P08-1283
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存环境侦测、可见性、生成和装备幸运输入。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：killGuide（第 2166 行）； killClothier（第 2168 行）； equipmentBasedLuckBonus（第 2170 行）；
/// lastEquipmentBasedLuckBonus（第 2172 行）； hasCreditsSceneMusicBox（第 2174 行）； findTreasure（第 2182
/// 行）； biomeSight（第 2184 行）； invis（第 2186 行）； detectCreature（第 2188 行）； nightVision（第 2190 行）；
/// enemySpawns（第 2192 行）； insideUnbreakableWalls（第 2208 行）； CanSeeInvisibleBlocks（第 2210 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 89 行。</para>
/// </remarks>
public sealed class PlayerEnvironmentDetectionAndSpawnStateComponent
{
  public bool KillGuide { get; internal set; }

  public bool KillClothier { get; internal set; }

  public float EquipmentBasedLuckBonus { get; internal set; }

  public float LastEquipmentBasedLuckBonus { get; internal set; }

  public bool HasCreditsSceneMusicBox { get; internal set; }

  public bool FindTreasure { get; internal set; }

  public bool BiomeSight { get; internal set; }

  public bool IsInvisible { get; internal set; }

  public bool DetectCreature { get; internal set; }

  public bool NightVision { get; internal set; }

  public bool EnemySpawns { get; internal set; }

  public bool InsideUnbreakableWalls { get; internal set; }

  public bool CanSeeInvisibleBlocks { get; internal set; }
}
