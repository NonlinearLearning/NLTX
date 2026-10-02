namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryDropRegistration(
  NpcNetId NpcNetId,
  BestiaryDropRateView DropRate);
