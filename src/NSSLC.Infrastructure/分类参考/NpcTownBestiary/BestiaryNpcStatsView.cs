namespace Terraria.NpcTownBestiary;

public readonly record struct BestiaryNpcStatsView(
  NpcNetId NpcNetId,
  int Damage,
  int LifeMax,
  float MonetaryValue,
  int Defense,
  float KnockbackResist,
  bool HideStats);
