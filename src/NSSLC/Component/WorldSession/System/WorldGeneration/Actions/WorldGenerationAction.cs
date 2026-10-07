using System;

namespace Terraria.WorldGeneration.Actions;

public readonly record struct WorldGenerationAction(
  long GenerationId,
  long Sequence,
  WorldGenerationActionPayload Payload)
{
  public bool IsWellFormed =>
    GenerationId >= 0 &&
    Sequence >= 0 &&
    Payload.IsWellFormed;

  public void Validate()
  {
    if (GenerationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(GenerationId));
    }

    if (Sequence < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Sequence));
    }

    Payload.Validate();
  }
}
