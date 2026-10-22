namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Trusted hit modifiers and target revision captured by the server composition.</summary>
public sealed record ProjectileNpcHitFacts(
    long ExpectedNpcRevision, int HitDirection, bool Critical,
    bool ExpertMode, bool NoGravity, bool ShadowFlame);
