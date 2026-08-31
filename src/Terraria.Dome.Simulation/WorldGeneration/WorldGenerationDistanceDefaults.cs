using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldGenerationDistanceDefaults
{
  public WorldGenerationDistanceDefaults(
    int oceanDistance,
    int beachDistance,
    int shimmerSafetyDistance,
    int cactusWaterWidth,
    int cactusWaterHeight,
    int cactusWaterLimit,
    int infectionAndGrassSpreadOuterWorldBuffer)
  {
    ValidateNonNegative(oceanDistance, nameof(oceanDistance));
    ValidateNonNegative(beachDistance, nameof(beachDistance));
    ValidateNonNegative(shimmerSafetyDistance, nameof(shimmerSafetyDistance));
    ValidateNonNegative(cactusWaterWidth, nameof(cactusWaterWidth));
    ValidateNonNegative(cactusWaterHeight, nameof(cactusWaterHeight));
    ValidateNonNegative(cactusWaterLimit, nameof(cactusWaterLimit));
    ValidateNonNegative(
      infectionAndGrassSpreadOuterWorldBuffer,
      nameof(infectionAndGrassSpreadOuterWorldBuffer));

    OceanDistance = oceanDistance;
    BeachDistance = beachDistance;
    ShimmerSafetyDistance = shimmerSafetyDistance;
    CactusWaterWidth = cactusWaterWidth;
    CactusWaterHeight = cactusWaterHeight;
    CactusWaterLimit = cactusWaterLimit;
    InfectionAndGrassSpreadOuterWorldBuffer = infectionAndGrassSpreadOuterWorldBuffer;
  }

  public static WorldGenerationDistanceDefaults Version4 { get; } = new(
    oceanDistance: 250,
    beachDistance: 380,
    shimmerSafetyDistance: 150,
    cactusWaterWidth: 50,
    cactusWaterHeight: 25,
    cactusWaterLimit: 25,
    infectionAndGrassSpreadOuterWorldBuffer: 10);

  public int OceanDistance { get; }

  public int BeachDistance { get; }

  public int ShimmerSafetyDistance { get; }

  public int CactusWaterWidth { get; }

  public int CactusWaterHeight { get; }

  public int CactusWaterLimit { get; }

  public int InfectionAndGrassSpreadOuterWorldBuffer { get; }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
