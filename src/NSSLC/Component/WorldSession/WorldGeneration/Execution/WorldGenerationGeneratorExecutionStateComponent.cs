namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores immutable run context needed by deterministic generation execution.
/// </summary>
public sealed class WorldGenerationGeneratorExecutionStateComponent
{
  public WorldGenerationGeneratorExecutionStateComponent(int seed)
  {
    Seed = seed;
  }

  public int Seed { get; }
}
