using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldGenerationRuntimeState
{
  public WorldGenerationRuntimeState(
    long generationId,
    WorldGenerationStage stage,
    long nextSequence,
    WorldGenerationRandomSnapshot random)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (!Enum.IsDefined(stage))
    {
      throw new ArgumentOutOfRangeException(nameof(stage));
    }

    if (nextSequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(nextSequence));
    }

    GenerationId = generationId;
    Stage = stage;
    NextSequence = nextSequence;
    Random = random;
  }

  public long GenerationId { get; }

  public WorldGenerationStage Stage { get; }

  public long NextSequence { get; }

  public WorldGenerationRandomSnapshot Random { get; }

  public static WorldGenerationRuntimeState Create(
    WorldGenerationStateComponent state,
    GenerationCursorComponent cursor,
    int randomStreamVersion)
  {
    return new WorldGenerationRuntimeState(
      state.GenerationId,
      state.Stage,
      state.NextSequence,
      new WorldGenerationRandomSnapshot(cursor.RandomState, randomStreamVersion));
  }
}
