using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the world-generation lifecycle and diagnostic flags owned by P19.
/// </summary>
public sealed class WorldGenerationExecutionStateComponent
{
  public WorldGenerationExecutionStateComponent(
    bool generatingWorld = false,
    int smallConsecutivesFound = 0,
    int smallConsecutivesEliminated = 0,
    bool placingTraps = false)
  {
    ReplaceState(
      generatingWorld,
      smallConsecutivesFound,
      smallConsecutivesEliminated,
      placingTraps);
  }

  public bool GeneratingWorld { get; private set; }

  public int SmallConsecutivesFound { get; private set; }

  public int SmallConsecutivesEliminated { get; private set; }

  public bool PlacingTraps { get; private set; }

  internal void ReplaceState(
    bool generatingWorld,
    int smallConsecutivesFound,
    int smallConsecutivesEliminated,
    bool placingTraps)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(smallConsecutivesFound);
    ArgumentOutOfRangeException.ThrowIfNegative(smallConsecutivesEliminated);

    GeneratingWorld = generatingWorld;
    SmallConsecutivesFound = smallConsecutivesFound;
    SmallConsecutivesEliminated = smallConsecutivesEliminated;
    PlacingTraps = placingTraps;
  }
}
