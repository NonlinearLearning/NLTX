using System;

namespace Terraria.WorldGeneration.Components;

public sealed class DungeonSpecialRewardGenerationComponent
{
  public DungeonSpecialRewardGenerationComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public bool GeneratedShadowKey { get; private set; }

  public bool GeneratedRamRune { get; private set; }

  internal void ReplaceState(
    bool generatedShadowKey,
    bool generatedRamRune)
  {
    GeneratedShadowKey = generatedShadowKey;
    GeneratedRamRune = generatedRamRune;
  }

  internal void ResetState()
  {
    GeneratedShadowKey = false;
    GeneratedRamRune = false;
  }

  public DungeonSpecialRewardGenerationSnapshot CreateSnapshot()
  {
    return new DungeonSpecialRewardGenerationSnapshot(
      GenerationId,
      GeneratedShadowKey,
      GeneratedRamRune);
  }
}
