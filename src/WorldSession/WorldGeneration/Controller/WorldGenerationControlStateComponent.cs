using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores pause intent and the stable pass identity at which a run may pause.
/// </summary>
public sealed class WorldGenerationControlStateComponent
{
  public WorldGenerationControlStateComponent(
    bool paused = false,
    string? pauseAfterPassId = null)
  {
    ReplaceState(paused, pauseAfterPassId);
  }

  public bool Paused { get; private set; }

  public string? PauseAfterPassId { get; private set; }

  internal void ReplaceState(bool paused, string? pauseAfterPassId)
  {
    if (pauseAfterPassId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(pauseAfterPassId);
    }

    Paused = paused;
    PauseAfterPassId = paused ? null : pauseAfterPassId;
  }
}
