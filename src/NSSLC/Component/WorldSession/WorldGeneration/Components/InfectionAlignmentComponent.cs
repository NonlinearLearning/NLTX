using System;

namespace Terraria.WorldGeneration.Components;

public sealed class InfectionAlignmentComponent
{
  public InfectionAlignmentComponent(
    long generationId,
    bool crimsonLeft,
    bool flipInfections)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    CrimsonLeft = crimsonLeft;
    FlipInfections = flipInfections;
  }

  public long GenerationId { get; }

  public bool CrimsonLeft { get; }

  public bool FlipInfections { get; }

  public InfectionAlignmentSnapshot CreateSnapshot()
  {
    return new InfectionAlignmentSnapshot(
      GenerationId,
      CrimsonLeft,
      FlipInfections);
  }
}
