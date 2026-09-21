using System;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Passes;

public readonly record struct WorldLayerMetricsCalculationInput
{
  public WorldLayerMetricsCalculationInput(
    long generationId,
    WorldGenerationConfigurationDefinition configuration,
    int leftBeachEnd,
    int rightBeachStart,
    int flatBeachPadding,
    bool isRemixWorld,
    bool isDrunkWorld,
    bool isGoodWorld,
    bool isNoSurfaceWorld,
    bool isSurfaceInSpace)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (leftBeachEnd < 0 || leftBeachEnd > configuration.WorldWidth)
    {
      throw new ArgumentOutOfRangeException(nameof(leftBeachEnd));
    }

    if (rightBeachStart < 0 || rightBeachStart > configuration.WorldWidth)
    {
      throw new ArgumentOutOfRangeException(nameof(rightBeachStart));
    }

    if (leftBeachEnd > rightBeachStart)
    {
      throw new ArgumentException(
        "The left beach boundary cannot be after the right beach boundary.",
        nameof(rightBeachStart));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(flatBeachPadding);

    GenerationId = generationId;
    Configuration = configuration.CreateSnapshot();
    LeftBeachEnd = leftBeachEnd;
    RightBeachStart = rightBeachStart;
    FlatBeachPadding = flatBeachPadding;
    IsRemixWorld = isRemixWorld;
    IsDrunkWorld = isDrunkWorld;
    IsGoodWorld = isGoodWorld;
    IsNoSurfaceWorld = isNoSurfaceWorld;
    IsSurfaceInSpace = isSurfaceInSpace;
  }

  public long GenerationId { get; }

  public WorldGenerationConfigurationSnapshot Configuration { get; }

  public int LeftBeachEnd { get; }

  public int RightBeachStart { get; }

  public int FlatBeachPadding { get; }

  public bool IsRemixWorld { get; }

  public bool IsDrunkWorld { get; }

  public bool IsGoodWorld { get; }

  public bool IsNoSurfaceWorld { get; }

  public bool IsSurfaceInSpace { get; }
}
