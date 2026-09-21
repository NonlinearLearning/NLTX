namespace Terraria.NpcTownBestiary;

public enum BestiaryEntryUnlockState : byte
{
  NotKnownAtAll,
  CanShowPortraitOnly,
  CanShowStats,
  CanShowDropsWithoutDropRates,
  CanShowDropsWithDropRates
}
