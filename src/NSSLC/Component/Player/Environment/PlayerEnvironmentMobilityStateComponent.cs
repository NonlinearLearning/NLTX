namespace Terraria.Player.Environment;

// status: implemented
// componentId: PLAYER.COMP.ENVIRONMENT_MOBILITY_STATE
// source-members: P08-1240..P08-1245, P08-1258..P08-1260, P08-1266..P08-1268
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存环境移动、游泳、熔岩和生成范围相关能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：canFloatInWater（第 2124 行）； hasFloatingTube（第 2126 行）； frogLegJumpBoost（第 2128 行）；
/// skyStoneEffects（第 2130 行）； spawnMax（第 2132 行）； blockRange（第 2134 行）； jumpBoost（第 2160 行）；
/// noFallDmg（第 2162 行）； swimTime（第 2164 行）； lavaImmune（第 2176 行）； gills（第 2178 行）； slowFall（第
/// 2180 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 88 行。</para>
/// </remarks>
public sealed class PlayerEnvironmentMobilityStateComponent
{
  public bool CanFloatInWater { get; internal set; }

  public bool HasFloatingTube { get; internal set; }

  public bool FrogLegJumpBoost { get; internal set; }

  public bool SkyStoneEffects { get; internal set; }

  public bool SpawnMax { get; internal set; }

  public int BlockRange { get; internal set; }

  public bool JumpBoost { get; internal set; }

  public bool NoFallDamage { get; internal set; }

  public int SwimTime { get; internal set; }

  public bool LavaImmune { get; internal set; }

  public bool Gills { get; internal set; }

  public bool SlowFall { get; internal set; }
}
