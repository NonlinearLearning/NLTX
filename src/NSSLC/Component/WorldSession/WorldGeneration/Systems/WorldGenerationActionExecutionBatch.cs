using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Actions;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Immutable, generation-scoped action input prepared before the commit barrier.
/// </summary>
public sealed class WorldGenerationActionExecutionBatch
{
  internal WorldGenerationActionExecutionBatch(
    long generationId,
    IReadOnlyList<WorldGenerationAction> actions)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    ArgumentNullException.ThrowIfNull(actions);
    GenerationId = generationId;
    Actions = actions;
  }

  public long GenerationId { get; }

  public IReadOnlyList<WorldGenerationAction> Actions { get; }

  public int Count => Actions.Count;
}
