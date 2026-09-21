using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete jungle chest and loot snapshot for its generation session.
/// </summary>
public static class JungleChestPlacementSystem
{
  public static void Commit(
    JungleChestAndLootGenerationStateComponent component,
    in JungleChestAndLootGenerationSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    ArgumentNullException.ThrowIfNull(snapshot.ChestXPositions);
    ArgumentNullException.ThrowIfNull(snapshot.ChestYPositions);

    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Jungle chest state cannot be committed to another generation.",
        nameof(snapshot));
    }

    if (snapshot.Capacity != JungleChestAndLootGenerationStateComponent.Capacity)
    {
      throw new ArgumentException(
        "Jungle chest capacity cannot change during a generation.",
        nameof(snapshot));
    }

    if (snapshot.Count < 0 || snapshot.Count > snapshot.Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(snapshot));
    }

    if (snapshot.ChestXPositions.Count != snapshot.Count ||
        snapshot.ChestYPositions.Count != snapshot.Count)
    {
      throw new ArgumentException(
        "Jungle chest coordinates must cover the complete used range.",
        nameof(snapshot));
    }

    component.ReplaceState(
      snapshot.JungleItemCount,
      snapshot.GennedLivingMahoganyWands,
      snapshot.Count,
      snapshot.ChestXPositions,
      snapshot.ChestYPositions);
  }
}
