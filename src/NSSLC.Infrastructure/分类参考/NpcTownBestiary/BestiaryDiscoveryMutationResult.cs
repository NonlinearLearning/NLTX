namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryDiscoveryMutationResult(
  bool Applied,
  bool Changed,
  int Value);
