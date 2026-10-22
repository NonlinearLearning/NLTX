namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-720, P09-721, P09-722, P09-723
// crossSubsystemOwner: environment ticking and derived capability projections remain integration-review
/// <summary>
/// 保存玩家呼吸资源、熔岩耐受计时和液体观察能力。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：breathMax（第 1037 行）； breath（第 1039 行）； lavaMax（第 1041 行）； lavaTime（第 1043 行）；
/// ignoreWater（第 1045 行）； lavaVision（第 1047 行）； lavaOpacity（第 1049 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P09-player-inventory-equipment-component-design.md。
/// </para>
/// <para>依据位置：第 137 行。</para>
/// <para>
/// 用途：环境流程读取剩余呼吸和熔岩耐受时间，决定资源消耗与危险效果；视野流程读取熔岩观察能力和透明度输入。
/// </para>
/// <para>
/// 当前实现：呼吸默认 200，熔岩透明度默认 1。常量给出透明度规则范围 0.4 到 1，属性本身没有自动限制；资源消耗、恢复和伤害由外部流程处理。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C140，L2（通用可选玩法）。分级理由：区域、游泳、呼吸、熔岩耐受、风力及表面状态属于额外环境玩法。
/// </para>
/// </remarks>
public sealed class PlayerResourceStateComponent
{
  /// <summary>
  /// 默认呼吸上限 200。
  /// </summary>
  public const int DefaultBreathMax = 200;

  /// <summary>
  /// 熔岩透明度的规则下限 0.4。
  /// </summary>
  public const float MinimumLavaOpacity = 0.4f;

  /// <summary>
  /// 熔岩透明度的规则上限 1。
  /// </summary>
  public const float MaximumLavaOpacity = 1f;

  /// <summary>
  /// 当前呼吸资源上限，默认 200。
  /// </summary>
  public int BreathMax { get; internal set; } = DefaultBreathMax;

  /// <summary>
  /// 当前剩余呼吸资源，默认 200。
  /// </summary>
  public int Breath { get; internal set; } = DefaultBreathMax;

  /// <summary>
  /// 熔岩耐受计时的上限输入。
  /// </summary>
  public int LavaMax { get; internal set; }

  /// <summary>
  /// 当前熔岩耐受计时，由外部环境流程推进。
  /// </summary>
  public int LavaTime { get; internal set; }

  /// <summary>
  /// 是否启用忽略水相关影响的能力。
  /// </summary>
  public bool IgnoreWater { get; internal set; }

  /// <summary>
  /// 是否启用熔岩观察能力。
  /// </summary>
  public bool LavaVision { get; internal set; }

  /// <summary>
  /// 熔岩显示透明度输入，默认 1。
  /// </summary>
  public float LavaOpacity { get; internal set; } = MaximumLavaOpacity;
}
