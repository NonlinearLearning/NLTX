using System;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileSpawnSystem
{
  public Entity Spawn(Arch.Core.World world, SpawnProjectileCommand command, ProjectileDefinition definition,
    int identity)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (command.ProjectileType != definition.ProjectileType)
    {
      throw new ArgumentException("Spawn command and definition types must match.", nameof(command));
    }

    return world.Create(
      new ProjectileTagComponent(),
      new ProjectileDefinitionComponent(
        definition.ProjectileType,
        definition.BehaviorId,
        definition.Damage,
        definition.LifetimeTicks,
        definition.Collider,
        definition.Friendly,
        definition.Hostile),
      new ProjectileBehaviorComponent(
        definition.BehaviorId,
        new ProjectileBehaviorState(0.0f, 0.0f, 0, 0)),
      new ProjectileNetworkIdentityComponent(command.Owner, identity, null),
      new ProjectilePenetrationComponent(command.MaximumPenetration),
      new TransformComponent(command.X, command.Y),
      new VelocityComponent(command.Facing * 4.0f, command.InitialVelocityY),
      definition.Collider,
      new ProjectileOwnerComponent(command.Owner),
      new ProjectileDamageComponent(command.Damage),
      new ProjectileLifetimeComponent(command.LifetimeTicks));
  }
}
