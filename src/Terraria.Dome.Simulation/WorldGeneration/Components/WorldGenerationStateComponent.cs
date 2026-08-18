using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public struct WorldGenerationStateComponent
{
  public WorldGenerationStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    Stage = WorldGenerationStage.Created;
    NextSequence = 0;
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
