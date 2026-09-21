namespace Terraria.NpcTownBestiary;

public readonly record struct NpcPersonalityEvaluationResult(
  bool Found,
  NpcAffectionLevel BestAffection,
  ShoppingBiomeDefinition? BestBiome);
