namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores a queued terminal abort request for the current generation run.
/// </summary>
public sealed class WorldGenerationAbortStateComponent
{
  public WorldGenerationAbortStateComponent(bool abortQueued = false)
  {
    AbortQueued = abortQueued;
  }

  public bool AbortQueued { get; private set; }
}
