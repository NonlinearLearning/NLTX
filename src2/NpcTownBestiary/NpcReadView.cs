namespace Terraria.NpcTownBestiary;

public readonly record struct NpcReadView(
  NpcEntityId EntityId,
  NpcTypeId Type,
  string Name = "",
  bool IsActive = true,
  int Damage = 0,
  int LifeMax = 0,
  float MonetaryValue = 0,
  int Defense = 0,
  float KnockbackResist = 0);
