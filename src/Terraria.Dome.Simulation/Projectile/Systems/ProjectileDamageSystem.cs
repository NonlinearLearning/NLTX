using System;
using System.Collections.Generic;
using Arch.Core;
using World = Arch.Core.World;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.Combat.Events;
using Terraria.Dome.Simulation.Combat.Systems;
using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileDamageSystem
{
  private const int HitImmunityTicks = 1;
  private const int OwnerNpcHitImmunityTicks = 10;
  private const int PlayerHitImmunityTicks = 40;
  private readonly HitImmunitySystem _immunitySystem = new();

  public IReadOnlyList<DamageRequestedEvent> Resolve(
    Arch.Core.World world,
    IEnumerable<DamageRequestedEvent> candidates,
    HitImmunityComponent immunity,
    Func<PlayerHandle, int>? ownerMeleeHitCooldownTicks = null)
  {
    List<DamageRequestedEvent> ordered = new(candidates);
    ordered.Sort(static (left, right) =>
    {
      int projectileComparison = left.ProjectileIdentity.CompareTo(right.ProjectileIdentity);
      return projectileComparison != 0
        ? projectileComparison
        : left.TargetIdentity.CompareTo(right.TargetIdentity);
    });

    List<DamageRequestedEvent> accepted = new();
    for (int index = 0; index < ordered.Count; index++)
    {
      DamageRequestedEvent candidate = ordered[index];
      if (!world.IsAlive(candidate.Projectile) || !world.IsAlive(candidate.Target) ||
          !world.Has<ProjectilePenetrationComponent>(candidate.Projectile) ||
          !CanDealDamage(world, candidate.Projectile) ||
          (world.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
            world.Has<NpcTagComponent>(candidate.Target) &&
            world.Get<ProjectileDefinitionComponent>(candidate.Projectile).UsesStaticNpcImmunity &&
            _immunitySystem.IsStaticNpcImmune(
              immunity,
              world.Get<ProjectileDefinitionComponent>(candidate.Projectile).ProjectileType,
              candidate.TargetIdentity)) ||
          (world.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
            world.Has<NpcTagComponent>(candidate.Target) &&
            world.Get<ProjectileDefinitionComponent>(candidate.Projectile).UsesLocalNpcImmunity &&
            _immunitySystem.IsNpcProjectileImmune(
              immunity,
              candidate.ProjectileIdentity,
              candidate.TargetIdentity)) ||
          (world.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
            world.Has<ProjectileOwnerComponent>(candidate.Projectile) &&
            world.Has<NpcTagComponent>(candidate.Target) &&
            CaresForAttackCooldown(world, candidate.Projectile) &&
            _immunitySystem.IsOwnerMeleeNpcImmune(
              immunity,
              world.Get<ProjectileOwnerComponent>(candidate.Projectile).Owner.Value,
              candidate.TargetIdentity)) ||
          (world.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
            world.Has<ProjectileOwnerComponent>(candidate.Projectile) &&
            world.Has<NpcTagComponent>(candidate.Target) &&
            UsesOwnerNpcImmunityFallback(
              world.Get<ProjectileDefinitionComponent>(candidate.Projectile),
              world.Get<ProjectilePenetrationComponent>(candidate.Projectile)
                .RemainingPenetration == 1) &&
            _immunitySystem.IsOwnerNpcImmune(
              immunity,
              world.Get<ProjectileOwnerComponent>(candidate.Projectile).Owner.Value,
              candidate.TargetIdentity)) ||
          (world.Has<PlayerTagComponent>(candidate.Target) &&
            _immunitySystem.IsPlayerProjectileImmune(
              immunity,
              candidate.ProjectileIdentity,
              candidate.TargetIdentity)))
      {
        continue;
      }

      ref ProjectilePenetrationComponent penetration =
        ref world.Get<ProjectilePenetrationComponent>(candidate.Projectile);
      bool isSingleHit = penetration.RemainingPenetration == 1;
      if (penetration.RemainingPenetration < -1 || penetration.RemainingPenetration == 0)
      {
        continue;
      }

      if (penetration.RemainingPenetration > 0)
      {
        penetration.RemainingPenetration--;
        if (penetration.RemainingPenetration == 0 &&
            world.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
            world.Get<ProjectileDefinitionComponent>(candidate.Projectile)
              .StopsDealingDamageAfterPenetrateHits)
        {
          penetration.RemainingPenetration = -1;
          ProjectileDamageComponent damage =
            world.Get<ProjectileDamageComponent>(candidate.Projectile);
          world.Set(candidate.Projectile, damage with { Amount = 0 });
        }
      }

      if (!world.Has<ProjectileDefinitionComponent>(candidate.Projectile))
      {
        if (world.Has<PlayerTagComponent>(candidate.Target))
        {
          _immunitySystem.ApplyPlayerProjectile(
            immunity,
            candidate.ProjectileIdentity,
            candidate.TargetIdentity,
            HitImmunityTicks);
        }
        else
        {
          _immunitySystem.ApplyProjectile(
            immunity,
            candidate.ProjectileIdentity,
            candidate.TargetIdentity,
            HitImmunityTicks);
        }
      }
      else if (world.Has<NpcTagComponent>(candidate.Target) &&
        world.Get<ProjectileDefinitionComponent>(candidate.Projectile).UsesLocalNpcImmunity)
      {
        int cooldownTicks = world.Get<ProjectileDefinitionComponent>(candidate.Projectile)
          .LocalNpcHitCooldownTicks;
        if (cooldownTicks == -1)
        {
          _immunitySystem.ApplyProjectilePermanent(
            immunity,
            candidate.ProjectileIdentity,
            candidate.TargetIdentity);
        }
        else if (cooldownTicks >= 0)
        {
          _immunitySystem.ApplyProjectile(
            immunity,
            candidate.ProjectileIdentity,
            candidate.TargetIdentity,
            cooldownTicks);
        }
      }
      if (world.Has<PlayerTagComponent>(candidate.Target))
      {
        _immunitySystem.ApplyPlayerProjectile(
          immunity,
          candidate.ProjectileIdentity,
          candidate.TargetIdentity,
          PlayerHitImmunityTicks);
      }
      if (world.Has<ProjectileDefinitionComponent>(candidate.Projectile) &&
          world.Has<NpcTagComponent>(candidate.Target))
      {
        ProjectileDefinitionComponent definition =
          world.Get<ProjectileDefinitionComponent>(candidate.Projectile);
        if (definition.UsesStaticNpcImmunity && definition.StaticNpcHitCooldownTicks > 0 &&
            (definition.AppliesImmunityTimeOnSingleHits || !isSingleHit))
        {
          _immunitySystem.ApplyStaticNpc(
            immunity,
            definition.ProjectileType,
            candidate.TargetIdentity,
            definition.StaticNpcHitCooldownTicks);
        }

        if (world.Has<ProjectileOwnerComponent>(candidate.Projectile) &&
            UsesOwnerNpcImmunityFallback(definition, isSingleHit))
        {
          _immunitySystem.ApplyOwnerNpc(
            immunity,
            world.Get<ProjectileOwnerComponent>(candidate.Projectile).Owner.Value,
            candidate.TargetIdentity,
            OwnerNpcHitImmunityTicks);
        }

        if (CaresForAttackCooldown(world, candidate.Projectile) &&
            ownerMeleeHitCooldownTicks != null)
        {
          PlayerHandle owner = world.Get<ProjectileOwnerComponent>(candidate.Projectile).Owner;
          int cooldownTicks = ownerMeleeHitCooldownTicks.Invoke(owner);
          if (cooldownTicks > 0)
          {
            _immunitySystem.ApplyOwnerMeleeNpc(
              immunity,
              owner.Value,
              candidate.TargetIdentity,
              cooldownTicks);
          }
        }
      }

      if (world.Has<ProjectileDamageComponent>(candidate.Projectile))
      {
        ProjectileDamageComponent damage =
          world.Get<ProjectileDamageComponent>(candidate.Projectile);
        world.Set(candidate.Projectile, damage.RegisterHit());
      }

      accepted.Add(candidate);
    }

    return accepted;
  }

  private static bool CanDealDamage(Arch.Core.World world, Entity projectile)
  {
    if (!world.Has<ProjectileDefinitionComponent>(projectile))
    {
      return true;
    }

    ProjectileDefinitionComponent definition =
      world.Get<ProjectileDefinitionComponent>(projectile);
    ProjectileBehaviorComponent behavior = world.Has<ProjectileBehaviorComponent>(projectile)
      ? world.Get<ProjectileBehaviorComponent>(projectile)
      : default;
    return ProjectileDamageEligibilityPolicy.CanDealDamage(definition, behavior);
  }

  private static bool UsesOwnerNpcImmunityFallback(
    ProjectileDefinitionComponent definition,
    bool isSingleHit)
  {
    return definition.UsesLocalNpcImmunity && definition.LocalNpcHitCooldownTicks == -2 &&
      (!isSingleHit || definition.AppliesImmunityTimeOnSingleHits);
  }

  private static bool CaresForAttackCooldown(Arch.Core.World world, Entity projectile)
  {
    if (!world.Has<ProjectileDefinitionComponent>(projectile) ||
        !world.Has<ProjectileOwnerComponent>(projectile))
    {
      return false;
    }

    ProjectileDefinitionComponent definition = world.Get<ProjectileDefinitionComponent>(projectile);
    ProjectileOwnerComponent owner = world.Get<ProjectileOwnerComponent>(projectile);
    return ProjectileAttackCooldownPolicy.CaresForAttackCooldown(
      definition.UsesOwnerMeleeHitCooldown,
      world.Has<NpcProjectileOwnerComponent>(projectile),
      world.Has<ProjectileTrapComponent>(projectile),
      owner.Owner.Value);
  }
}
