using MinimalImplementation.Combat.Npcs;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectileNpcHitResult(
    OwnerCommitResult Result, NpcView? Npc, int AppliedDamage,
    ProjectileCollisionResolutionResult? Projectile, NpcDamagePacketIntent? DamagePacket = null) {
  public bool IsApplied => Result.Status == OwnerCommitStatus.Applied && Npc is not null;

  /// <summary>Projectile was consumed; retry the same plan before the next owner update.</summary>
  public bool HasPendingDamage => !IsApplied && Projectile is { IsApplied: true };
}
