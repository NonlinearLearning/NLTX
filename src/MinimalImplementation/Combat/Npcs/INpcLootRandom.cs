namespace MinimalImplementation.Combat.Npcs;

/// <summary>Explicit loot effects, bound to the authoritative closest player's luck.</summary>
public interface INpcLootRandom {
  int RollLuck(int denominator);

  int Next(int minimumInclusive, int maximumExclusive);

  float NextFloat();
}
