namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryUICollectionSnapshot(
  BestiaryEntryKey OwnerEntryKey,
  BestiaryEntryUnlockState UnlockState,
  int DisplayIndex);
