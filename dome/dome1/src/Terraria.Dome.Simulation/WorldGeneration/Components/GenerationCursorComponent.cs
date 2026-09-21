using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct GenerationCursorComponent
{
  public GenerationCursorComponent(
    WorldGenerationStage stage,
    int sectionX,
    int sectionY,
    uint randomState)
  {
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

    Stage = stage;
    SectionX = sectionX;
    SectionY = sectionY;
    RandomState = randomState;
  }

  public WorldGenerationStage Stage { get; }

  public int SectionX { get; }

  public int SectionY { get; }

  public uint RandomState { get; }

  public GenerationCursorComponent Advance(
    WorldGenerationStage stage,
    int sectionX,
    int sectionY,
    uint randomState)
  {
    GenerationCursorComponent next = new(stage, sectionX, sectionY, randomState);
    if (stage < Stage ||
        (stage == Stage &&
          (sectionX < SectionX || (sectionX == SectionX && sectionY < SectionY))))
    {
      throw new ArgumentException(
        "A generation cursor cannot move backwards.",
        nameof(stage));
    }

    return next;
  }
}
