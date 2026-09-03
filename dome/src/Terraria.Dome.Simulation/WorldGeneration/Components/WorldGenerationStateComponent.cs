using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public struct WorldGenerationStateComponent
{
  public WorldGenerationStateComponent(long generationId, long nextSequence = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (nextSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(nextSequence));
    }

    GenerationId = generationId;
    Stage = WorldGenerationStage.Created;
    NextSequence = nextSequence;
    IsComplete = false;
  }

  public long GenerationId { get; }
  public WorldGenerationStage Stage { get; private set; }
  public long NextSequence { get; private set; }
  public bool IsComplete { get; private set; }

  public long ReserveSequence()
  {
    if (IsComplete)
    {
      throw new InvalidOperationException("A completed world cannot reserve a sequence.");
    }

    if (NextSequence == long.MaxValue)
    {
      throw new InvalidOperationException(
        "A world generation cannot reserve a sequence after exhausting its sequence space.");
    }

    long sequence = NextSequence;
    NextSequence++;
    return sequence;
  }

  public bool TryAdvance(WorldGenerationStage nextStage)
  {
    if (nextStage <= Stage)
    {
      return false;
    }

    Stage = nextStage;
    IsComplete = nextStage == WorldGenerationStage.Validated;
    return true;
  }
}
