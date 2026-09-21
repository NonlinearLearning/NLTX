using System;

namespace Terraria.WorldGeneration.Components;

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
