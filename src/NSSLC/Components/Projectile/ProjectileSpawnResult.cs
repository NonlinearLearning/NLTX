using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>Authoritative result returned by a lifecycle spawn operation.</summary>
public readonly record struct ProjectileSpawnResult(
    ProjectileLifecycleStatus Status,
    EntityRuntimeHandle Handle,
    ArchEntity Entity,
    string? Detail)
{
  public bool Succeeded => Status == ProjectileLifecycleStatus.Applied;
}
