using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界地表、岩层、云层和雪区边界指标。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：lowestCloud（第 46 行）； worldSurfaceLow（第 66 行）； worldSurface（第 68 行）； worldSurfaceHigh（第
/// 70 行）； rockLayerLow（第 72 行）； rockLayer（第 74 行）； rockLayerHigh（第 76 行）； snowTop（第 78 行）；
/// snowBottom（第 80 行）； snowOriginLeft（第 82 行）； snowOriginRight（第 84 行）； snowMinX（第 86 行）；
/// snowMaxX（第 88 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 388 行。</para>
/// <para>
/// 用途：生成与环境查询流程读取地表和岩层参考值、雪区边界及云层位置，确定后续地形步骤的工作范围。
/// </para>
/// <para>
/// 当前实现：构造和 ReplaceMetrics 校验地表与岩层值有限，并复制等长的雪区边界序列。CreateSnapshot 再次复制序列，快照保持创建时的指标。
/// </para>
/// <para>
/// 范围含义：这些值保存生成流程的层级结果，具体坐标方向、采样索引和分层规则由外部生成算法定义。
/// </para>
/// <para>
/// 原始分级记录（2026-10-08）：C198，L2（通用可选玩法）。分级理由：生态采样、结构保留区、地层统计、扫描和地形变换适用于可编辑／动态世界，固定小地图可减少。
/// </para>
/// </remarks>
public sealed class WorldLayerMetricsComponent
{
  private int[] _snowMinX;
  private int[] _snowMaxX;
  private IReadOnlyList<int> _snowMinXView;
  private IReadOnlyList<int> _snowMaxXView;

  public WorldLayerMetricsComponent(
    long generationId,
    int lowestCloud = -1,
    double worldSurfaceLow = 0.0d,
    double worldSurface = 0.0d,
    double worldSurfaceHigh = 0.0d,
    double rockLayerLow = 0.0d,
    double rockLayer = 0.0d,
    double rockLayerHigh = 0.0d,
    int snowTop = 0,
    int snowBottom = 0,
    int snowOriginLeft = 0,
    int snowOriginRight = 0,
    IReadOnlyList<int>? snowMinX = null,
    IReadOnlyList<int>? snowMaxX = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ValidateFinite(worldSurfaceLow, nameof(worldSurfaceLow));
    ValidateFinite(worldSurface, nameof(worldSurface));
    ValidateFinite(worldSurfaceHigh, nameof(worldSurfaceHigh));
    ValidateFinite(rockLayerLow, nameof(rockLayerLow));
    ValidateFinite(rockLayer, nameof(rockLayer));
    ValidateFinite(rockLayerHigh, nameof(rockLayerHigh));

    GenerationId = generationId;
    LowestCloud = lowestCloud;
    WorldSurfaceLow = worldSurfaceLow;
    WorldSurface = worldSurface;
    WorldSurfaceHigh = worldSurfaceHigh;
    RockLayerLow = rockLayerLow;
    RockLayer = rockLayer;
    RockLayerHigh = rockLayerHigh;
    SnowTop = snowTop;
    SnowBottom = snowBottom;
    SnowOriginLeft = snowOriginLeft;
    SnowOriginRight = snowOriginRight;
    (_snowMinX, _snowMaxX) = CopySnowColumns(snowMinX, snowMaxX);
    _snowMinXView = Array.AsReadOnly(_snowMinX);
    _snowMaxXView = Array.AsReadOnly(_snowMaxX);
  }

  /// <summary>
  /// 所属世界生成会话编号。
  /// </summary>
  public long GenerationId { get; }

  /// <summary>
  /// 记录的最低云层位置，默认 -1。
  /// </summary>
  public int LowestCloud { get; private set; }

  /// <summary>
  /// 地表层低值边界。
  /// </summary>
  public double WorldSurfaceLow { get; private set; }

  /// <summary>
  /// 地表层参考位置。
  /// </summary>
  public double WorldSurface { get; private set; }

  /// <summary>
  /// 地表层高值边界。
  /// </summary>
  public double WorldSurfaceHigh { get; private set; }

  /// <summary>
  /// 岩层低值边界。
  /// </summary>
  public double RockLayerLow { get; private set; }

  /// <summary>
  /// 岩层参考位置。
  /// </summary>
  public double RockLayer { get; private set; }

  /// <summary>
  /// 岩层高值边界。
  /// </summary>
  public double RockLayerHigh { get; private set; }

  /// <summary>
  /// 雪区顶部边界。
  /// </summary>
  public int SnowTop { get; private set; }

  /// <summary>
  /// 雪区底部边界。
  /// </summary>
  public int SnowBottom { get; private set; }

  /// <summary>
  /// 雪区起始左边界。
  /// </summary>
  public int SnowOriginLeft { get; private set; }

  /// <summary>
  /// 雪区起始右边界。
  /// </summary>
  public int SnowOriginRight { get; private set; }

  /// <summary>
  /// 雪区横向最小边界的只读序列，与 SnowMaxX 等长。
  /// </summary>
  public IReadOnlyList<int> SnowMinX => _snowMinXView;

  /// <summary>
  /// 雪区横向最大边界的只读序列，与 SnowMinX 等长。
  /// </summary>
  public IReadOnlyList<int> SnowMaxX => _snowMaxXView;

  /// <summary>
  /// 校验并整体替换分层指标，同时复制雪区边界序列。
  /// </summary>
  internal void ReplaceMetrics(
    int lowestCloud,
    double worldSurfaceLow,
    double worldSurface,
    double worldSurfaceHigh,
    double rockLayerLow,
    double rockLayer,
    double rockLayerHigh,
    int snowTop,
    int snowBottom,
    int snowOriginLeft,
    int snowOriginRight,
    IReadOnlyList<int>? snowMinX,
    IReadOnlyList<int>? snowMaxX)
  {
    ValidateFinite(worldSurfaceLow, nameof(worldSurfaceLow));
    ValidateFinite(worldSurface, nameof(worldSurface));
    ValidateFinite(worldSurfaceHigh, nameof(worldSurfaceHigh));
    ValidateFinite(rockLayerLow, nameof(rockLayerLow));
    ValidateFinite(rockLayer, nameof(rockLayer));
    ValidateFinite(rockLayerHigh, nameof(rockLayerHigh));
    (int[] copiedSnowMinX, int[] copiedSnowMaxX) =
      CopySnowColumns(snowMinX, snowMaxX);

    LowestCloud = lowestCloud;
    WorldSurfaceLow = worldSurfaceLow;
    WorldSurface = worldSurface;
    WorldSurfaceHigh = worldSurfaceHigh;
    RockLayerLow = rockLayerLow;
    RockLayer = rockLayer;
    RockLayerHigh = rockLayerHigh;
    SnowTop = snowTop;
    SnowBottom = snowBottom;
    SnowOriginLeft = snowOriginLeft;
    SnowOriginRight = snowOriginRight;
    _snowMinX = copiedSnowMinX;
    _snowMaxX = copiedSnowMaxX;
    _snowMinXView = Array.AsReadOnly(_snowMinX);
    _snowMaxXView = Array.AsReadOnly(_snowMaxX);
  }

  /// <summary>
  /// 复制当前分层指标与雪区序列，返回独立读取快照。
  /// </summary>
  public WorldLayerMetricsSnapshot CreateSnapshot()
  {
    return new WorldLayerMetricsSnapshot(
      GenerationId,
      LowestCloud,
      WorldSurfaceLow,
      WorldSurface,
      WorldSurfaceHigh,
      RockLayerLow,
      RockLayer,
      RockLayerHigh,
      SnowTop,
      SnowBottom,
      SnowOriginLeft,
      SnowOriginRight,
      CopyReadOnly(_snowMinX),
      CopyReadOnly(_snowMaxX));
  }

  private static (int[] SnowMinX, int[] SnowMaxX) CopySnowColumns(
    IReadOnlyList<int>? snowMinX,
    IReadOnlyList<int>? snowMaxX)
  {
    int minLength = snowMinX?.Count ?? 0;
    int maxLength = snowMaxX?.Count ?? 0;
    if (minLength != maxLength)
    {
      throw new ArgumentException(
        "Snow column bounds must contain matching numbers of minimum and maximum entries.",
        nameof(snowMaxX));
    }

    int[] copiedSnowMinX = new int[minLength];
    int[] copiedSnowMaxX = new int[maxLength];
    for (int index = 0; index < minLength; index++)
    {
      copiedSnowMinX[index] = snowMinX![index];
      copiedSnowMaxX[index] = snowMaxX![index];
    }

    return (copiedSnowMinX, copiedSnowMaxX);
  }

  private static IReadOnlyList<int> CopyReadOnly(IReadOnlyList<int> source)
  {
    int[] copy = new int[source.Count];
    for (int index = 0; index < source.Count; index++)
    {
      copy[index] = source[index];
    }

    return Array.AsReadOnly(copy);
  }

  private static void ValidateFinite(double value, string parameterName)
  {
    if (!double.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
