namespace MinimalImplementation.Combat.Npcs;

/// <summary>Server AI randomness, independent of the nearest player's loot luck.</summary>
public interface INpcAiRandom {
  int Next(int minimumInclusive, int maximumExclusive);
}
