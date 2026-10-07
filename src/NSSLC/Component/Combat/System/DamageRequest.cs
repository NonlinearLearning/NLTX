using Terraria.Relationships;

namespace Terraria.Combat;

public readonly record struct DamageRequest(
  EntityReference Attacker,
  EntityReference Target,
  int Amount,
  int Defense,
  float Knockback,
  int HitDirection,
  bool Critical,
  int CooldownTicks,
  DamageAttribution Attribution);
