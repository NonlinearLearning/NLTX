using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacySandPatchesPassDefinition(
  double Density,
  int RemixInvocationDivisor,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive,
  int TileType)
{
  public int CalculateInvocationCount(int width, bool isRemixWorld)
  {
    if (width < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int count = checked((int)(width * Density));
    return isRemixWorld ? count / RemixInvocationDivisor : count;
  }

  public (int MinimumInclusive, int MaximumExclusive) GetInitialYRange(
    int height,
    LegacyTerrainRuntimeProfile profile,
    bool isRemixWorld)
  {
    ArgumentNullException.ThrowIfNull(profile);
    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    int minimumY = isRemixWorld
      ? checked((int)profile.RockLayer - 100)
      : checked((int)profile.WorldSurface);
    int maximumYExclusive = isRemixWorld
      ? checked(height - 350)
      : checked((int)profile.RockLayer);
    ValidateYRange(minimumY, maximumYExclusive, height);
    return (minimumY, maximumYExclusive);
  }

  public (int MinimumInclusive, int MaximumExclusive) GetRetryYRange(
    LegacyTerrainRuntimeProfile profile)
  {
    ArgumentNullException.ThrowIfNull(profile);
    int minimumY = checked((int)profile.WorldSurface);
    int maximumYExclusive = checked((int)profile.RockLayer);
    ValidateYRange(minimumY, maximumYExclusive, int.MaxValue);
    return (minimumY, maximumYExclusive);
  }

  public void Validate()
  {
    if (!double.IsFinite(Density) || Density < 0 || RemixInvocationDivisor <= 0 ||
        MinimumStrength < 0 || MaximumStrengthExclusive <= MinimumStrength ||
        MinimumSteps < 0 || MaximumStepsExclusive <= MinimumSteps || TileType < 0)
    {
      throw new InvalidOperationException(
        "SandPatches pass definition contains an invalid source contract.");
    }
  }

  public static LegacySandPatchesPassDefinition CreateDefault()
  {
    return new LegacySandPatchesPassDefinition(
      Density: 0.013,
      RemixInvocationDivisor: 4,
      MinimumStrength: 15,
      MaximumStrengthExclusive: 70,
      MinimumSteps: 20,
      MaximumStepsExclusive: 130,
      TileType: 53);
  }

  private static void ValidateYRange(int minimumY, int maximumYExclusive, int height)
  {
    if (minimumY < 0 || maximumYExclusive <= minimumY || maximumYExclusive > height)
    {
      throw new ArgumentOutOfRangeException(nameof(minimumY));
    }
  }
}
