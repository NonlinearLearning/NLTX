namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryDropRateView(
  int ItemType,
  float DropRate,
  int StackMin,
  int StackMax);
