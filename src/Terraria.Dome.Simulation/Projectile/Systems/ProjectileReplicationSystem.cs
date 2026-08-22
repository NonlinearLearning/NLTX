using System;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileReplicationSystem
{
  public ProjectileReplicationSnapshot Project(
    Entity entity,
    Arch.Core.World world,
    int replicationId,
    long revision,
    WorldSectionCoordinates section)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (replicationId <= 0 || revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(replicationId));
    }

    if (!world.IsAlive(entity))
    {
      throw new ArgumentException("Projectile entity is not alive in the supplied world.", nameof(entity));
    }

    TransformComponent transform = world.Get<TransformComponent>(entity);
    VelocityComponent velocity = world.Get<VelocityComponent>(entity);
    ProjectileOwnerComponent owner = world.Get<ProjectileOwnerComponent>(entity);
    ProjectileDamageComponent damage = world.Get<ProjectileDamageComponent>(entity);
    ProjectileLifetimeComponent lifetime = world.Get<ProjectileLifetimeComponent>(entity);
    ProjectileDefinitionComponent definition = world.Get<ProjectileDefinitionComponent>(entity);
    ProjectileNetworkIdentityComponent identity = world.Get<ProjectileNetworkIdentityComponent>(entity);
    return new ProjectileReplicationSnapshot(
      replicationId,
      definition.ProjectileType,
      owner.Owner,
      new SimulationVector(transform.X, transform.Y),
      new SimulationVector(velocity.X, velocity.Y),
      damage.Amount,
      lifetime.RemainingTicks,
      true,
      revision,
      section,
      identity.Identity,
      identity.ProjectileUuid);
  }
}
