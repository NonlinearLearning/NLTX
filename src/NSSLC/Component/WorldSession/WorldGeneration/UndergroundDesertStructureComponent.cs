using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存地下沙漠与蜂巢区域的边界。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：UndergroundDesertLocation（第 138 行）； UndergroundDesertHiveLocation（第 140 行）；
/// desertHiveHigh（第 142 行）； desertHiveLow（第 144 行）； desertHiveLeft（第 146 行）； desertHiveRight（第
/// 148 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 804 行。</para>
/// </remarks>
public sealed class UndergroundDesertStructureComponent
{
  public UndergroundDesertStructureComponent(
    long generationId,
    UndergroundDesertRectangle undergroundDesertLocation = default,
    UndergroundDesertRectangle undergroundDesertHiveLocation = default,
    int desertHiveHigh = 0,
    int desertHiveLow = 0,
    int desertHiveLeft = 0,
    int desertHiveRight = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    ReplaceLayout(
      undergroundDesertLocation,
      undergroundDesertHiveLocation,
      desertHiveHigh,
      desertHiveLow,
      desertHiveLeft,
      desertHiveRight);
  }

  public long GenerationId { get; }

  public UndergroundDesertRectangle UndergroundDesertLocation { get; private set; }

  public UndergroundDesertRectangle UndergroundDesertHiveLocation { get; private set; }

  public int DesertHiveHigh { get; private set; }

  public int DesertHiveLow { get; private set; }

  public int DesertHiveLeft { get; private set; }

  public int DesertHiveRight { get; private set; }

  public void ReplaceLayout(
    UndergroundDesertRectangle undergroundDesertLocation,
    UndergroundDesertRectangle undergroundDesertHiveLocation,
    int desertHiveHigh,
    int desertHiveLow,
    int desertHiveLeft,
    int desertHiveRight)
  {
    UndergroundDesertLocation = undergroundDesertLocation;
    UndergroundDesertHiveLocation = undergroundDesertHiveLocation;
    DesertHiveHigh = desertHiveHigh;
    DesertHiveLow = desertHiveLow;
    DesertHiveLeft = desertHiveLeft;
    DesertHiveRight = desertHiveRight;
  }

  public UndergroundDesertStructureSnapshot CreateSnapshot()
  {
    return new UndergroundDesertStructureSnapshot(
      GenerationId,
      UndergroundDesertLocation,
      UndergroundDesertHiveLocation,
      DesertHiveHigh,
      DesertHiveLow,
      DesertHiveLeft,
      DesertHiveRight);
  }
}
