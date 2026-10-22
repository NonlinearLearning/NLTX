namespace Terraria.Player.Environment;

// status: implemented
// componentId: PLAYER.COMP.ZONE_AND_ENVIRONMENT_STATE
// source-members: P08-662, P08-665..P08-670
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存玩家区域位集、环境效果免疫计时和微光区域历史标记。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>
/// 主要源成员：environmentBuffImmunityTimer（第 917 行）； zone1（第 923 行）； zone2（第 925 行）； zone3（第 927 行）；
/// zone4（第 929 行）； zone5（第 931 行）； _wasInShimmerZone（第 933 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P08-player-environment-armor-component-design.md。</para>
/// <para>依据位置：第 105 行。</para>
/// <para>
/// 用途：环境求解或网络同步提交区域字节，生成、音乐、效果与环境交互流程读取玩家所在区域。
/// </para>
/// <para>
/// 当前实现：五个 byte 保留原版 BitsByte 位集。ApplyNetworkZones 只覆盖这五组区域位，不修改免疫计时和微光历史；位含义、计时推进及区域求解
/// 由外部流程定义。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C101，L2（通用可选玩法）。分级理由：区域、游泳、呼吸、熔岩耐受、风力及表面状态属于额外环境玩法。
/// </para>
/// </remarks>
public sealed class PlayerZoneAndEnvironmentStateComponent
{
  /// <summary>
  /// 环境 Buff 免疫的计时状态。
  /// </summary>
  public int EnvironmentBuffImmunityTimer { get; internal set; }

  // The legacy BitsByte values are kept as bytes until the protocol type is available in NLTX.
  /// <summary>
  /// 第一组原版区域标记位。
  /// </summary>
  public byte Zone1 { get; internal set; }

  /// <summary>
  /// 第二组原版区域标记位。
  /// </summary>
  public byte Zone2 { get; internal set; }

  /// <summary>
  /// 第三组原版区域标记位。
  /// </summary>
  public byte Zone3 { get; internal set; }

  /// <summary>
  /// 第四组原版区域标记位。
  /// </summary>
  public byte Zone4 { get; internal set; }

  /// <summary>
  /// 第五组原版区域标记位。
  /// </summary>
  public byte Zone5 { get; internal set; }

  /// <summary>
  /// 之前是否处于微光区域的历史标记。
  /// </summary>
  public bool WasInShimmerZone { get; internal set; }

  /// <summary>
  /// 整体覆盖五组区域字节，保留其他环境状态。
  /// </summary>
  public void ApplyNetworkZones(byte zone1, byte zone2, byte zone3, byte zone4, byte zone5)
  {
    Zone1 = zone1;
    Zone2 = zone2;
    Zone3 = zone3;
    Zone4 = zone4;
    Zone5 = zone5;
  }

}
