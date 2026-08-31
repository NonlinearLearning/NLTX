using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldGenerationLifecycleSnapshot
{
  public WorldGenerationLifecycleSnapshot(bool isGenerating, bool isGeneratingOrLoading)
  {
    if (isGenerating && !isGeneratingOrLoading)
    {
      throw new ArgumentException(
        "A generating world must also be marked as generating or loading.",
        nameof(isGeneratingOrLoading));
    }

    IsGenerating = isGenerating;
    IsGeneratingOrLoading = isGeneratingOrLoading;
  }

  public bool IsGenerating { get; }

  public bool IsGeneratingOrLoading { get; }
}
