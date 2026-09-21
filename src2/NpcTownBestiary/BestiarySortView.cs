namespace Terraria.NpcTownBestiary;

public readonly record struct BestiarySortView(
  BestiaryEntryDefinition Entry,
  BestiaryEntryUnlockState UnlockState,
  int StatValue);
