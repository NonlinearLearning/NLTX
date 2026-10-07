using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界生成的海滩边界、沙层和贝壳放置参数。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.WorldBuilding.GenVars。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.WorldBuilding/GenVars.cs。</para>
/// <para>
/// 主要源成员：leftBeachEnd（第 90 行）； rightBeachStart（第 92 行）； beachBordersWidth（第 94 行）；
/// beachSandRandomCenter（第 96 行）； beachSandRandomWidthRange（第 98 行）； beachSandDungeonExtraWidth（第
/// 100 行）； beachSandJungleExtraWidth（第 102 行）； shellStartXLeft（第 104 行）； shellStartYLeft（第 106
/// 行）； shellStartXRight（第 108 行）； shellStartYRight（第 110 行）； oceanWaterStartRandomMin（第 112 行）。
/// </para>
/// <para>重组说明：GenerationId 用于拆分后的版本或实例生命周期管理。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>
/// 拆分依据文件：2026-09-11-version4-p17-world-terrain-biomes-genvars-component-design.md。
/// </para>
/// <para>依据位置：第 589 行。</para>
/// </remarks>
public sealed class BeachBoundaryComponent
{
  public BeachBoundaryComponent(
    long generationId,
    int leftBeachEnd = 0,
    int rightBeachStart = 0,
    int beachBordersWidth = 0,
    int beachSandRandomCenter = 0,
    int beachSandRandomWidthRange = 0,
    int beachSandDungeonExtraWidth = 0,
    int beachSandJungleExtraWidth = 0,
    int shellStartXLeft = 0,
    int shellStartYLeft = 0,
    int shellStartXRight = 0,
    int shellStartYRight = 0,
    int oceanWaterStartRandomMin = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    ReplaceBoundaries(
      leftBeachEnd,
      rightBeachStart,
      beachBordersWidth,
      beachSandRandomCenter,
      beachSandRandomWidthRange,
      beachSandDungeonExtraWidth,
      beachSandJungleExtraWidth,
      shellStartXLeft,
      shellStartYLeft,
      shellStartXRight,
      shellStartYRight,
      oceanWaterStartRandomMin);
  }

  public long GenerationId { get; }

  public int LeftBeachEnd { get; private set; }

  public int RightBeachStart { get; private set; }

  public int BeachBordersWidth { get; private set; }

  public int BeachSandRandomCenter { get; private set; }

  public int BeachSandRandomWidthRange { get; private set; }

  public int BeachSandDungeonExtraWidth { get; private set; }

  public int BeachSandJungleExtraWidth { get; private set; }

  public int ShellStartXLeft { get; private set; }

  public int ShellStartYLeft { get; private set; }

  public int ShellStartXRight { get; private set; }

  public int ShellStartYRight { get; private set; }

  public int OceanWaterStartRandomMin { get; private set; }

  internal void ReplaceBoundaries(
    int leftBeachEnd,
    int rightBeachStart,
    int beachBordersWidth,
    int beachSandRandomCenter,
    int beachSandRandomWidthRange,
    int beachSandDungeonExtraWidth,
    int beachSandJungleExtraWidth,
    int shellStartXLeft,
    int shellStartYLeft,
    int shellStartXRight,
    int shellStartYRight,
    int oceanWaterStartRandomMin)
  {
    LeftBeachEnd = leftBeachEnd;
    RightBeachStart = rightBeachStart;
    BeachBordersWidth = beachBordersWidth;
    BeachSandRandomCenter = beachSandRandomCenter;
    BeachSandRandomWidthRange = beachSandRandomWidthRange;
    BeachSandDungeonExtraWidth = beachSandDungeonExtraWidth;
    BeachSandJungleExtraWidth = beachSandJungleExtraWidth;
    ShellStartXLeft = shellStartXLeft;
    ShellStartYLeft = shellStartYLeft;
    ShellStartXRight = shellStartXRight;
    ShellStartYRight = shellStartYRight;
    OceanWaterStartRandomMin = oceanWaterStartRandomMin;
  }

  public BeachBoundarySnapshot CreateSnapshot()
  {
    return new BeachBoundarySnapshot(
      GenerationId,
      LeftBeachEnd,
      RightBeachStart,
      BeachBordersWidth,
      BeachSandRandomCenter,
      BeachSandRandomWidthRange,
      BeachSandDungeonExtraWidth,
      BeachSandJungleExtraWidth,
      ShellStartXLeft,
      ShellStartYLeft,
      ShellStartXRight,
      ShellStartYRight,
      OceanWaterStartRandomMin);
  }
}
