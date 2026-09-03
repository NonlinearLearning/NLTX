using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldGenerationLifecycleSnapshot
{
  public WorldGenerationLifecycleSnapshot(
    bool isGenerating,
    bool isGeneratingOrLoading,
    WorldGenerationStage stage = WorldGenerationStage.Created,
    int sectionX = 0,
    int sectionY = 0,
    string? failureReason = null)
  {
    if (isGenerating && !isGeneratingOrLoading)
    {
      throw new ArgumentException(
        "A generating world must also be marked as generating or loading.",
        nameof(isGeneratingOrLoading));
    }

    if (!Enum.IsDefined(stage))
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    if (sectionX < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionX));
    }

    if (sectionY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionY));
    }

    IsGenerating = isGenerating;
    IsGeneratingOrLoading = isGeneratingOrLoading;
    Stage = stage;
    SectionX = sectionX;
    SectionY = sectionY;
    FailureReason = failureReason;
  }

  public bool IsGenerating { get; }

  public bool IsGeneratingOrLoading { get; }

  public WorldGenerationStage Stage { get; }

  public int SectionX { get; }

  public int SectionY { get; }

  public string? FailureReason { get; }
}
