namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores whether a generation run pauses when a committed pass hash differs.
/// </summary>
public sealed class WorldGenerationHashMismatchPolicyComponent
{
  public WorldGenerationHashMismatchPolicyComponent(bool pauseOnHashMismatch = true)
  {
    PauseOnHashMismatch = pauseOnHashMismatch;
  }

  public bool PauseOnHashMismatch { get; private set; }
}
