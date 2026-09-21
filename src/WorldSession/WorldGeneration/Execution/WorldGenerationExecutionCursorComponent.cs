using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the stable identity of the pass currently being executed.
/// </summary>
public sealed class WorldGenerationExecutionCursorComponent
{
  public WorldGenerationExecutionCursorComponent(string? currentPassId = null)
  {
    ReplaceState(currentPassId);
  }

  public string? CurrentPassId { get; private set; }

  public bool HasCurrentPass => CurrentPassId is not null;

  internal void ReplaceState(string? currentPassId)
  {
    if (currentPassId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(currentPassId);
    }

    CurrentPassId = currentPassId;
  }
}
