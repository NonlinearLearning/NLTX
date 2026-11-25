using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Authoritative result returned by a lifecycle spawn operation.</summary>
public readonly record struct ProjectileSpawnResult(
    ProjectileLifecycleStatus Status,
    EntityRuntimeHandle Handle,
    ArchEntity Entity,
    string? Detail,
    EcsServerEntityCreationTicket? Creation = null)
{
  public bool Succeeded => Status == ProjectileLifecycleStatus.Applied;

  public bool IsDeferred => Status == ProjectileLifecycleStatus.Deferred;
}
