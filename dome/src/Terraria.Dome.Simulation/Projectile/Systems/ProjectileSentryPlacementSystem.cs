using System;
using System.Collections.Generic;
using Arch.Core;
using ArchWorld = Arch.Core.World;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Definitions;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileSentryPlacementSystem
{
  private readonly QueryDescription _sentryQuery = new QueryDescription()
    .WithAll<ProjectileSentryComponent, LocationComponent, ColliderComponent,
      ProjectileLifetimeComponent>();

  public bool CanPlace(
    ArchWorld world,
    float x,
    float y,
    ProjectileDefinition definition,
    ProjectileDefinitionRegistry definitions,
    IReadOnlyList<SpawnProjectileCommand> pendingCommands)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(pendingCommands);
    if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(definition.Scale) ||
        definition.Scale <= 0.0f)
    {
      return false;
    }

    ColliderComponent candidateCollider = ScaleCollider(definition.Collider, definition.Scale);
    bool overlapsActiveSentry = false;
    LocationComponent candidateTransform = new(x, y);
    world.Query(
      in _sentryQuery,
      (Entity entity, ref LocationComponent transform, ref ColliderComponent collider,
        ref ProjectileLifetimeComponent lifetime) =>
      {
        if (overlapsActiveSentry || lifetime.RemainingTicks <= 0 ||
            !float.IsFinite(transform.X) || !float.IsFinite(transform.Y) ||
            !float.IsFinite(collider.Width) || !float.IsFinite(collider.Height) ||
            collider.Width <= 0.0f || collider.Height <= 0.0f)
        {
          return;
        }

        if (Overlaps(candidateTransform, candidateCollider, transform, collider))
        {
          overlapsActiveSentry = true;
        }
      });
    if (overlapsActiveSentry)
    {
      return false;
    }

    for (int index = 0; index < pendingCommands.Count; index++)
    {
      SpawnProjectileCommand command = pendingCommands[index];
      if (!definitions.TryGet(command.ProjectileType, out ProjectileDefinition pendingDefinition) ||
          (!command.IsSentry && !pendingDefinition.IsSentry) ||
          !float.IsFinite(command.X) || !float.IsFinite(command.Y))
      {
        continue;
      }

      ColliderComponent pendingCollider = ScaleCollider(
        pendingDefinition.Collider,
        pendingDefinition.Scale);
      if (Overlaps(
            candidateTransform,
            candidateCollider,
            new LocationComponent(command.X, command.Y),
            pendingCollider))
      {
        return false;
      }
    }

    return true;
  }

  private static ColliderComponent ScaleCollider(ColliderComponent collider, float scale)
  {
    return new ColliderComponent(collider.Width * scale, collider.Height * scale);
  }

  private static bool Overlaps(
    LocationComponent firstTransform,
    ColliderComponent firstCollider,
    LocationComponent secondTransform,
    ColliderComponent secondCollider)
  {
    return firstTransform.X < secondTransform.X + secondCollider.Width &&
      firstTransform.X + firstCollider.Width > secondTransform.X &&
      firstTransform.Y < secondTransform.Y + secondCollider.Height &&
      firstTransform.Y + firstCollider.Height > secondTransform.Y;
  }
}
