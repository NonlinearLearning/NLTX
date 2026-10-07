namespace Terraria.Npc;

public readonly record struct NpcDamageOverTimeRequest(
  int Amount,
  long Tick,
  NpcEntityIdentityComponent TextSourceIdentity,
  NpcDamageOverTimeTextBounds TextBounds,
  bool Immortal = false,
  bool IsBoss = false);
