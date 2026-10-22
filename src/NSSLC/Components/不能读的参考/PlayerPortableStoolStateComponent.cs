namespace Terraria.Player;

/// <summary>
/// 保存玩家便携凳能力、使用状态以及高度和显示偏移。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.PortableStoolUsage。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/PortableStoolUsage.cs。</para>
/// <para>
/// 主要源成员：HasAStool（第 5 行）； IsInUse（第 7 行）； HeightBoost（第 9 行）； VisualYOffset（第 11 行）；
/// MapYOffset（第 13 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 480 行。</para>
/// <para>
/// 用途：便携凳操作流程登记是否使用及高度变化，移动、表现和地图流程读取相应偏移。
/// </para>
/// <para>
/// 当前实现：只保存已提交状态。ResetForLifecycle 清除能力、使用标记和三个偏移；使用资格和实际位置或绘制变化由外部流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C138，L2（通用可选玩法）。分级理由：统计金币损失、坐下、床睡眠和休息加成是死亡重生以外的玩法。
/// </para>
/// </remarks>
public sealed class PlayerPortableStoolStateComponent
{
  /// <summary>
  /// 玩家是否具备便携凳能力。
  /// </summary>
  public bool HasAStool { get; internal set; }

  /// <summary>
  /// 便携凳当前是否正在使用。
  /// </summary>
  public bool IsInUse { get; internal set; }

  /// <summary>
  /// 便携凳提供的高度增量输入。
  /// </summary>
  public int HeightBoost { get; internal set; }

  /// <summary>
  /// 便携凳相关的视觉垂直偏移。
  /// </summary>
  public int VisualYOffset { get; internal set; }

  /// <summary>
  /// 便携凳相关的地图垂直偏移。
  /// </summary>
  public int MapYOffset { get; internal set; }

  /// <summary>
  /// 清除便携凳能力、使用状态和所有偏移。
  /// </summary>
  internal void ResetForLifecycle()
  {
    HasAStool = false;
    IsInUse = false;
    HeightBoost = 0;
    VisualYOffset = 0;
    MapYOffset = 0;
  }
}
