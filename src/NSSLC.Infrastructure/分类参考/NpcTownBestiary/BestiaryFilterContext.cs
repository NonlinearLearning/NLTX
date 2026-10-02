namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryFilterContext(
  BestiaryEntryDefinition Entry,
  BestiaryEntryUnlockState UnlockState,
  IReadOnlySet<string> InfoElementKeys,
  string SearchTerm);
