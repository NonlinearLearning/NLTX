using System;
using System.Collections.Generic;
using Arch.Core;
using ArchWorld = Arch.Core.World;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Definitions;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileSentryLimitSystem
{
  private const int MaximumDespawnCommandsPerReconcile = 1000;
  private readonly QueryDescription _sentryQuery = new QueryDescription()
    .WithAll<ProjectileSentryComponent, ProjectileOwnerComponent, ProjectileDefinitionComponent,
      ProjectileLifetimeComponent>();

  public IReadOnlyList<DespawnEntityCommand> CollectDespawnCommands(
    ArchWorld world,
    ProjectileStore projectileIdsByEntity,
    PlayerHandle owner,
    int maximumTurrets,
    bool eventActive)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(projectileIdsByEntity);
    ArgumentOutOfRangeException.ThrowIfNegative(maximumTurrets);
    if (!owner.IsValid)
    {
      return Array.Empty<DespawnEntityCommand>();
    }

    List<SentryCandidate> candidates = new();
    world.Query(
      in _sentryQuery,
      (Entity entity, ref ProjectileOwnerComponent projectileOwner,
        ref ProjectileDefinitionComponent definition, ref ProjectileLifetimeComponent lifetime) =>
      {
        if (projectileOwner.Owner != owner || lifetime.RemainingTicks <= 0 ||
            !projectileIdsByEntity.TryGetValue(entity, out int replicationId) ||
            !ProjectileTurretPersistencePolicy.CanWipe(
              definition.ProjectileType,
              projectileOwner.Owner,
              owner,
              isSentry: true,
              eventActive: eventActive,
              isDd2Summon: world.Has<ProjectileDd2SummonComponent>(entity)))
        {
          return;
        }

        candidates.Add(new SentryCandidate(entity, replicationId, lifetime.RemainingTicks));
      });

    int removalCount = candidates.Count - maximumTurrets;
    if (removalCount <= 0)
    {
      return Array.Empty<DespawnEntityCommand>();
    }

    candidates.Sort(SentryCandidateComparer.Instance);
    removalCount = Math.Min(removalCount, MaximumDespawnCommandsPerReconcile);
    List<DespawnEntityCommand> commands = new(removalCount);
    for (int index = 0; index < removalCount; index++)
    {
      commands.Add(new DespawnEntityCommand(
        candidates[index].Entity,
        ProjectileTombstoneReason.Administrative));
    }

    return commands;
  }

  private readonly record struct SentryCandidate(
    Entity Entity,
    int ReplicationId,
    int RemainingTicks);

  private sealed class SentryCandidateComparer : IComparer<SentryCandidate>
  {
    public static readonly SentryCandidateComparer Instance = new();

    public int Compare(SentryCandidate first, SentryCandidate second)
    {
      int remainingTicks = first.RemainingTicks.CompareTo(second.RemainingTicks);
      return remainingTicks != 0
        ? remainingTicks
        : first.ReplicationId.CompareTo(second.ReplicationId);
    }
  }
}
