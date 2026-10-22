using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界地形层级、生态区域位置及生成后的地表特征。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldGen。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/WorldGen.cs。</para>
/// <para>主要源成员：beachDistance（第 4109 行）。</para>
/// <para>重组说明：GenerationId、TerrainRevision 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/non-authoritative/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-non-authoritative-P03-world-generation-dungeons-component-design.md。
/// </para>
/// <para>依据位置：第 209 行。</para>
/// <para>
/// 用途：生成流程提交地狱、海洋、海滩和主要生态区域结果，后续步骤与世界查询读取这些地形事实。
/// </para>
/// <para>
/// 当前实现：GenerationId 构造时要求非负，其余属性在初始化时保存。可空位置和标记允许表达尚未提供的结果，实际地形求解由外部生成流程负责。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C201，L2（通用可选玩法）。分级理由：生态采样、结构保留区、地层统计、扫描和地形变换适用于可编辑／动态世界，固定小地图可减少。
/// </para>
/// </remarks>
public sealed class WorldTerrainStateComponent
{
  public WorldTerrainStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 此地形状态结果的版本。
  /// </summary>
  public ulong TerrainRevision { get; init; }

  /// <summary>
  /// 地狱层纵向位置，可为空。
  /// </summary>
  public int? UnderworldLayerY { get; init; }

  /// <summary>
  /// 海洋水位纵向位置，可为空。
  /// </summary>
  public int? OceanLevelY { get; init; }

  /// <summary>
  /// 海滩距离或范围输入，可为空。
  /// </summary>
  public int? BeachDistance { get; init; }

  /// <summary>
  /// 海滩沙层深度输入，可为空。
  /// </summary>
  public int? BeachSandDepth { get; init; }

  /// <summary>
  /// 地表层偏移输入，可为空。
  /// </summary>
  public double? SurfaceOffset { get; init; }

  /// <summary>
  /// 丛林区域的记录位置，可为空。
  /// </summary>
  public TilePosition? Jungle { get; init; }

  /// <summary>
  /// 雪区的记录位置，可为空。
  /// </summary>
  public TilePosition? Snow { get; init; }

  /// <summary>
  /// 沙漠区域的记录位置，可为空。
  /// </summary>
  public TilePosition? Desert { get; init; }

  /// <summary>
  /// 地表是否为沙漠的结果，可为空。
  /// </summary>
  public bool? SurfaceIsDesert { get; init; }

  /// <summary>
  /// 地表是否为蘑菇区域的结果，可为空。
  /// </summary>
  public bool? SurfaceIsMushrooms { get; init; }

  /// <summary>
  /// 地表是否位于太空区域的结果，可为空。
  /// </summary>
  public bool? SurfaceIsInSpace { get; init; }

  /// <summary>
  /// 出生点是否位于海洋的结果，可为空。
  /// </summary>
  public bool? IsOceanAtSpawn { get; init; }

  /// <summary>
  /// 出生点是否位于海滩的结果，可为空。
  /// </summary>
  public bool? IsBeachAtSpawn { get; init; }

  /// <summary>
  /// 是否没有地表的结果，可为空。
  /// </summary>
  public bool? IsNoSurface { get; init; }

  /// <summary>
  /// 是否使用 Remix 地形的结果，可为空。
  /// </summary>
  public bool? IsRemix { get; init; }

  /// <summary>
  /// 是否记录为异常世界的结果，可为空。
  /// </summary>
  public bool? IsErrorWorld { get; init; }
}
