namespace Terraria.NpcTownBestiary;

public readonly record struct BestiarySightCandidate(
  NpcNetId NpcNetId,
  BestiaryCreditId CreditId,
  BestiaryPlayerBounds Bounds);
