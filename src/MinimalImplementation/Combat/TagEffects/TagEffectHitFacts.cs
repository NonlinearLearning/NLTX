using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Combat.Npcs;

namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Trusted projectile/NPC facts captured before a tagged hit is committed.</summary>
public sealed record TagEffectHitFacts(
    byte Player,
    NpcView Npc,
    int Damage,
    short? ProjectileType,
    int BonusTagDamage,
    bool ProjectileOwned,
    bool IsMinion,
    bool IsMinionShot,
    bool IsSentry,
    bool Critical)
{
  public CombatEntityIdentity Entity => Npc.Entity;
}
