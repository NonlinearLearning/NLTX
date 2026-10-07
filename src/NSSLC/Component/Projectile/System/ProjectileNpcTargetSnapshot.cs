namespace Terraria.Projectile;

/// <summary>
/// NPC facts read by projectile damage qualification for one target slot.
/// The fields map the NPC active/type/friendly/damage, AI, trap, immortality,
/// tile-collision, and Jellyfish-set facts read before Version4 Colliding.
/// OwnerImmune maps targetNPC.immune[projectile.owner] != 0.
/// NpcSlot is the target's Main.npc index used by static-immunity queries.
/// </summary>
public readonly record struct ProjectileNpcTargetSnapshot(
  int Type,
  bool Active,
  bool Friendly,
  bool DontTakeDamage,
  bool DontTakeDamageFromHostiles,
  int AiStyle,
  float Ai2,
  bool TrapImmune,
  bool Immortal,
  bool NoTileCollide,
  bool IsZappingJellyfish,
  bool OwnerImmune,
  int NpcSlot = -1);
