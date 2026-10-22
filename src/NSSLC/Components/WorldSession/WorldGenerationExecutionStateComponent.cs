using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成执行中的工作标记和小连续区域处理统计。
/// </summary>
/// <remarks>
/// <para>职责：保存世界生成正在执行、连续小块处理和陷阱放置状态。</para>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>
/// 主要源成员：generatingWorld（第 4287 行）； SmallConsecutivesFound（第 4302 行）；
/// SmallConsecutivesEliminated（第 4304 行）； placingTraps（第 4326 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-P19-world-generation-execution-component-design.md。
/// </para>
/// <para>依据位置：第 19 行。</para>
/// <para>
/// 用途：生成步骤维护是否生成世界、是否放置陷阱及发现和消除小连续区域的数量，其他控制流程读取进度事实。
/// </para>
/// <para>
/// 当前实现：ReplaceState 要求两种数量非负，再整体保存状态；没有实际地形算法。陷阱放置和区域处理由外部生成步骤执行。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C189，L2（通用可选玩法）。分级理由：生成器的运行阶段、步骤计划、随机流、选择与取消仅在服务端生成世界时需要，预置地图可省去。
/// </para>
/// </remarks>
public sealed class WorldGenerationExecutionStateComponent
{
  public WorldGenerationExecutionStateComponent(
    bool generatingWorld = false,
    int smallConsecutivesFound = 0,
    int smallConsecutivesEliminated = 0,
    bool placingTraps = false)
  {
    ReplaceState(
      generatingWorld,
      smallConsecutivesFound,
      smallConsecutivesEliminated,
      placingTraps);
  }

  /// <summary>
  /// 生成执行流程当前是否在生成世界。
  /// </summary>
  public bool GeneratingWorld { get; private set; }

  /// <summary>
  /// 原版 smallConsecutivesFound 对应的小连续区域发现计数。
  /// </summary>
  public int SmallConsecutivesFound { get; private set; }

  /// <summary>
  /// 原版 smallConsecutivesEliminated 对应的小连续区域消除计数。
  /// </summary>
  public int SmallConsecutivesEliminated { get; private set; }

  /// <summary>
  /// 生成流程当前是否处于放置陷阱阶段。
  /// </summary>
  public bool PlacingTraps { get; private set; }

  /// <summary>
  /// 校验两种计数非负后整体提交生成工作状态。
  /// </summary>
  internal void ReplaceState(
    bool generatingWorld,
    int smallConsecutivesFound,
    int smallConsecutivesEliminated,
    bool placingTraps)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(smallConsecutivesFound);
    ArgumentOutOfRangeException.ThrowIfNegative(smallConsecutivesEliminated);

    GeneratingWorld = generatingWorld;
    SmallConsecutivesFound = smallConsecutivesFound;
    SmallConsecutivesEliminated = smallConsecutivesEliminated;
    PlacingTraps = placingTraps;
  }
}
