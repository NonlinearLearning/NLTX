namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record JungleChestItemSelectionResult(
  int BaseItemType,
  int JungleItemCount,
  int NextJungleItemCount,
  bool RandomOverrideDeferred,
  bool CounterMutationDeferred);
