namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores whether the current pause was caused by a generation hash mismatch.
/// </summary>
public sealed class WorldGenerationHashMismatchStateComponent
{
  public WorldGenerationHashMismatchStateComponent(bool pausedDueToHashMismatch = false)
  {
    PausedDueToHashMismatch = pausedDueToHashMismatch;
  }

  public bool PausedDueToHashMismatch { get; private set; }
}
