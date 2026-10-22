namespace Terraria.Player.Environment;

// status: implemented
// componentId: PLAYER.COMP.ENVIRONMENT_MOBILITY_STATE
// source-members: P08-1240..P08-1245, P08-1258..P08-1260, P08-1266..P08-1268
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存玩家水中移动、跳跃、防坠落和环境相关能力输入。
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
/// <para>
/// 用途：装备与环境效果汇总游泳、浮水、鳃和熔岩免疫等能力，移动与生存流程据此应用规则。
/// </para>
/// <para>
/// 当前实现：只有内部可写属性。能力重建、SwimTime 的计时、实际移动和环境伤害由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C100，L2（通用可选玩法）。分级理由：区域、游泳、呼吸、熔岩耐受、风力及表面状态属于额外环境玩法。
/// </para>
/// </remarks>
public sealed class PlayerEnvironmentMobilityStateComponent
{
  /// <summary>
  /// 是否具备在水中漂浮的能力。
  /// </summary>
  public bool CanFloatInWater { get; internal set; }

  /// <summary>
  /// 是否具备浮圈相关能力。
  /// </summary>
  public bool HasFloatingTube { get; internal set; }

  /// <summary>
  /// 蛙腿跳跃加成是否生效。
  /// </summary>
  public bool FrogLegJumpBoost { get; internal set; }

  /// <summary>
  /// 天界石相关效果是否生效。
  /// </summary>
  public bool SkyStoneEffects { get; internal set; }

  /// <summary>
  /// 原版 spawnMax 生成规则标记。
  /// </summary>
  public bool SpawnMax { get; internal set; }

  /// <summary>
  /// 方块交互范围的附加输入。
  /// </summary>
  public int BlockRange { get; internal set; }

  /// <summary>
  /// 是否启用跳跃加成。
  /// </summary>
  public bool JumpBoost { get; internal set; }

  /// <summary>
  /// 是否免疫坠落伤害。
  /// </summary>
  public bool NoFallDamage { get; internal set; }

  /// <summary>
  /// 游泳相关计时，更新规则由外部移动流程定义。
  /// </summary>
  public int SwimTime { get; internal set; }

  /// <summary>
  /// 是否具备熔岩免疫能力。
  /// </summary>
  public bool LavaImmune { get; internal set; }

  /// <summary>
  /// 是否启用鳃的水下呼吸能力。
  /// </summary>
  public bool Gills { get; internal set; }

  /// <summary>
  /// 是否启用缓降能力。
  /// </summary>
  public bool SlowFall { get; internal set; }
}
