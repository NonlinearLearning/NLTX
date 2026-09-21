namespace Terraria.NpcTownBestiary;

public readonly record struct NpcBiomePreference(
  NpcAffectionLevel Affection,
  ShoppingBiomeDefinition Biome);
