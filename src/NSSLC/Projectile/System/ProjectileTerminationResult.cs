using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>Authoritative result returned by a lifecycle termination operation.</summary>
public readonly record struct ProjectileTerminationResult(
    ProjectileLifecycleStatus Status,
    EntityRuntimeHandle Handle,
    ProjectileTerminationReason? Reason,
    string? Detail)
{
  public bool Succeeded => Status == ProjectileLifecycleStatus.Applied;
}
