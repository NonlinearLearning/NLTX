using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct GenerationCursorComponent(
  WorldGenerationStage Stage,
  int SectionX,
  int SectionY,
  uint RandomState)
{
  public GenerationCursorComponent Advance(
    WorldGenerationStage stage,
    int sectionX,
    int sectionY,
    uint randomState)
  {
    if (stage < Stage)
    {
      throw new ArgumentException(
        "A generation cursor cannot move backwards.",
        nameof(stage));
    }

    return new GenerationCursorComponent(stage, sectionX, sectionY, randomState);
  }
}
