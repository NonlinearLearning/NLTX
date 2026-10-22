using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Server-resolved hit facts, after attacker authority and hit eligibility checks.</summary>
public sealed record NpcDamageRequest(
    CombatEntityIdentity Target,
    long ExpectedRevision,
    int Damage,
    float Knockback,
    int HitDirection,
    bool Critical,
    bool ExpertMode,
    bool NoGravity,
    bool ShadowFlame,
    SessionEpoch? InteractionEpoch);
