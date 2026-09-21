namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Result of one jungle-chest item selection before state commit.
/// </summary>
public readonly record struct JungleChestLootSelectionResult(
  int ItemType,
  int NextJungleItemCount);
