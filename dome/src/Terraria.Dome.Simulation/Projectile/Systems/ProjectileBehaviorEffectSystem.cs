using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Behaviors;

using EntityEcs.Components;
namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileBehaviorEffectSystem
{
  private const int Type304DecayStartTick = 30;
  private const int Type304DespawnTick = 55;
  private const int HitStatusDespawnTick = 60;
  private const int Type656DamageZeroLocalAiThreshold = 30;
  private const int Type697LocalAiThreshold = 42;
  private const int Type607 = 607;
  private const int Type645 = 645;
  private const int Type864 = 864;
  private const float Type656PrimaryClampStart = 780.0f;
  private const float Type656PrimaryClampModulo = 60.0f;
  private const float Type656PrimaryDespawnThreshold = 900.0f;
  private const float Type658InitialLocalAi0 = 0.8f;
  private const float Type658ChildSpawnLocalAi1 = 60.0f;
  private const float Type658DespawnLocalAi1 = 120.0f;
  private const float ColliderHalfExtentMultiplier = 0.5f;
  private const float TileCenterCoordinateOffset = 0.5f;
  private const float Type304DecayMultiplier = 0.9f;
  private const float Type357DecayMultiplier = 0.8f;
  private const float Type876DecayMultiplier = 0.9f;
  private const float Type876VelocityMultiplier = 0.6f;

  public void Apply(
    ref ProjectileDamageComponent damage,
    ref ProjectileDefinitionComponent definition,
    ref ProjectileLifetimeComponent lifetime,
    ProjectileBehaviorComponent behavior)
  {
    if (behavior.BehaviorId == LegacyAiStyle2HitStatusProjectileBehavior.Id &&
        behavior.State.Primary >= HitStatusDespawnTick)
    {
      lifetime.RemainingTicks = 0;
      return;
    }

    if (behavior.BehaviorId != LegacyAiStyle2Type304ProjectileBehavior.Id ||
        behavior.State.Primary < Type304DecayStartTick)
    {
      return;
    }

    damage = new ProjectileDamageComponent((int)(damage.Amount * Type304DecayMultiplier));
    definition = definition with
    {
      Knockback = (int)(definition.Knockback * Type304DecayMultiplier)
    };
    if (behavior.State.Primary >= Type304DespawnTick)
    {
      lifetime.RemainingTicks = 0;
    }
  }

  public bool ApplyType656Tick(
    ref ProjectileDamageComponent damage,
    ref ProjectileLifetimeComponent lifetime,
    ref ProjectileSoundDelayComponent soundDelay,
    ref ProjectileBehaviorComponent behavior,
    ref ProjectileNetworkUpdateComponent networkUpdate,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 656)
    {
      return false;
    }

    if (!float.IsFinite(behavior.State.LocalAi0) || !float.IsFinite(behavior.State.Primary))
    {
      return false;
    }

    float primary = behavior.State.Primary + 1.0f;
    behavior.State = behavior.State with { Primary = primary };
    if (soundDelay.RemainingTicks == 0)
    {
      soundDelay = new ProjectileSoundDelayComponent(-1);
    }
    if (primary >= Type656PrimaryDespawnThreshold)
    {
      lifetime.RemainingTicks = 0;
    }

    if (behavior.State.LocalAi0 < Type656DamageZeroLocalAiThreshold)
    {
      return true;
    }

    damage = damage with { Amount = 0 };
    if (primary >= Type656PrimaryClampStart)
    {
      return true;
    }

    float clampedPrimary = Type656PrimaryClampStart + (primary % Type656PrimaryClampModulo);
    behavior.State = behavior.State with { Primary = clampedPrimary };
    networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
    return true;
  }

  public bool ApplyType657Tick(
    ref ProjectileLifetimeComponent lifetime,
    ref ProjectileSoundDelayComponent soundDelay,
    ref ProjectileBehaviorComponent behavior,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 657 ||
        !float.IsFinite(behavior.State.Primary))
    {
      return false;
    }

    float primary = behavior.State.Primary + 1.0f;
    behavior.State = behavior.State with { Primary = primary };
    if (soundDelay.RemainingTicks == 0)
    {
      soundDelay = new ProjectileSoundDelayComponent(-1);
    }

    if (primary >= Type656PrimaryDespawnThreshold)
    {
      lifetime.RemainingTicks = 0;
    }

    return true;
  }

  public bool ApplyType658Tick(
    ref ProjectileLifetimeComponent lifetime,
    ref ProjectileSoundDelayComponent soundDelay,
    ref ProjectileBehaviorComponent behavior,
    ref VelocityComponent velocity,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 658 ||
        !float.IsFinite(behavior.State.LocalAi0) ||
        !float.IsFinite(behavior.State.LocalAi1) ||
        !float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y))
    {
      return false;
    }

    float nextLocalAi1 = behavior.State.LocalAi1 + 1.0f;
    if (!float.IsFinite(nextLocalAi1))
    {
      return false;
    }

    float localAi0 = behavior.State.LocalAi0 == 0.0f
      ? Type658InitialLocalAi0
      : behavior.State.LocalAi0;
    behavior.State = behavior.State with
    {
      LocalAi0 = localAi0,
      LocalAi1 = nextLocalAi1
    };
    if (soundDelay.RemainingTicks == 0)
    {
      soundDelay = new ProjectileSoundDelayComponent(-1);
    }

    velocity = new VelocityComponent(0.0f, 0.0f);
    if (nextLocalAi1 >= Type658DespawnLocalAi1)
    {
      lifetime.RemainingTicks = 0;
    }

    return true;
  }

  public bool ApplyType658TileCenterSnap(
    ref LocationComponent transform,
    ref ProjectileDirectionComponent direction,
    ref ProjectileBehaviorComponent behavior,
    ColliderComponent collider,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 658 ||
        !float.IsFinite(transform.X) || !float.IsFinite(transform.Y) ||
        !float.IsFinite(collider.Width) || collider.Width <= 0.0f ||
        !float.IsFinite(collider.Height) || collider.Height <= 0.0f ||
        !float.IsFinite(behavior.State.LocalAi0))
    {
      return false;
    }

    if (behavior.State.LocalAi0 != 0.0f)
    {
      return true;
    }

    float halfWidth = collider.Width * ColliderHalfExtentMultiplier;
    float halfHeight = collider.Height * ColliderHalfExtentMultiplier;
    float snappedCenterX = MathF.Floor(transform.X + halfWidth) +
      TileCenterCoordinateOffset;
    float snappedCenterY = MathF.Floor(transform.Y + halfHeight) +
      TileCenterCoordinateOffset;
    float nextX = snappedCenterX - halfWidth;
    float nextY = snappedCenterY - halfHeight;
    if (!float.IsFinite(halfWidth) || !float.IsFinite(halfHeight) ||
        !float.IsFinite(snappedCenterX) || !float.IsFinite(snappedCenterY) ||
        !float.IsFinite(nextX) || !float.IsFinite(nextY))
    {
      return false;
    }

    transform.X = nextX;
    transform.Y = nextY;
    direction.Horizontal = 1;
    behavior.State = behavior.State with { LocalAi0 = Type658InitialLocalAi0 };
    return true;
  }

  public bool ShouldSpawnType658Child(
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorComponent behavior)
  {
    return definition.ProjectileType == 658 &&
      float.IsFinite(behavior.State.LocalAi1) &&
      behavior.State.LocalAi1 == Type658ChildSpawnLocalAi1;
  }

  public bool ApplyAcceptedHit(
    ref ProjectileBehaviorComponent behavior,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType == 697)
    {
      if (!float.IsFinite(behavior.State.Primary) ||
          behavior.State.Primary < Type697LocalAiThreshold)
      {
        return false;
      }

      behavior.State = behavior.State with { LocalAi1 = 1.0f };
      return true;
    }

    if (definition.ProjectileType != 656)
    {
      return false;
    }

    float localAi0 = behavior.State.LocalAi0;
    if (!float.IsFinite(localAi0) || localAi0 >= float.MaxValue)
    {
      return false;
    }

    behavior.State = behavior.State with { LocalAi0 = localAi0 + 1.0f };
    return true;
  }

  public bool ApplyAcceptedHit(
    ref ProjectileBehaviorComponent behavior,
    ref ProjectileFriendlyStateComponent friendlyState,
    ref ProjectileNetworkUpdateComponent networkUpdate,
    ProjectileDefinitionComponent definition)
  {
    return ApplyAcceptedHit(
      ref behavior,
      ref friendlyState,
      ref networkUpdate,
      definition,
      1);
  }

  public bool ApplyAcceptedHit(
    ref ProjectileBehaviorComponent behavior,
    ref ProjectileFriendlyStateComponent friendlyState,
    ref ProjectileNetworkUpdateComponent networkUpdate,
    ProjectileDefinitionComponent definition,
    int penetration)
  {
    if (definition.ProjectileType == Type607)
    {
      if (!friendlyState.IsFriendly || !float.IsFinite(behavior.State.Primary))
      {
        return false;
      }

      behavior.State = behavior.State with { Primary = 1.0f };
      friendlyState.IsFriendly = false;
      networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
      return true;
    }

    if (definition.ProjectileType == 451)
    {
      if (penetration <= 0 || !float.IsFinite(behavior.State.Primary) ||
          !float.IsFinite(behavior.State.Secondary))
      {
        return false;
      }

      float primary = behavior.State.Primary == 0.0f
        ? behavior.State.Primary + penetration
        : behavior.State.Primary - penetration - 1.0f;
      behavior.State = behavior.State with { Primary = primary, Secondary = 0.0f };
      networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
      return true;
    }

    if (definition.ProjectileType == Type645)
    {
      if (!float.IsFinite(behavior.State.Primary) ||
          !float.IsFinite(behavior.State.Secondary) || behavior.State.Secondary == -1.0f)
      {
        return false;
      }

      behavior.State = behavior.State with { Primary = 0.0f, Secondary = -1.0f };
      networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
      return true;
    }

    if (definition.ProjectileType == Type864)
    {
      if (penetration <= 0 || !float.IsFinite(behavior.State.Primary) ||
          !float.IsFinite(behavior.State.Secondary))
      {
        return false;
      }

      if (behavior.State.Primary > 0.0f)
      {
        behavior.State = behavior.State with { Primary = -1.0f, Secondary = 0.0f };
        networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
        return true;
      }

      return false;
    }

    return ApplyAcceptedHit(ref behavior, definition);
  }

  public bool ApplyAcceptedHitPenetration(
    ref ProjectileBehaviorComponent behavior,
    ref ProjectileFriendlyStateComponent friendlyState,
    ref ProjectileNetworkUpdateComponent networkUpdate,
    ref ProjectilePenetrationComponent penetration,
    ref ProjectileDamageComponent damage,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 866 || penetration.RemainingPenetration != 0 ||
        !float.IsFinite(behavior.State.Primary) || !float.IsFinite(behavior.State.Secondary))
    {
      return false;
    }

    penetration.RemainingPenetration = 1;
    damage = damage with { Amount = 0 };
    behavior.State = behavior.State with { Secondary = -1.0f };
    networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
    return true;
  }

  public bool ApplyTileCollisionBehavior(
    ref ProjectileBehaviorComponent behavior,
    ref VelocityComponent velocity,
    ref ProjectileNetworkUpdateComponent networkUpdate,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType == Type645)
    {
      if (!float.IsFinite(behavior.State.Primary) || !float.IsFinite(behavior.State.Secondary))
      {
        return false;
      }

      behavior.State = behavior.State with { Primary = 0.0f, Secondary = -1.0f };
      networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
      return true;
    }

    if (definition.ProjectileType != 451 || !float.IsFinite(velocity.X) ||
        !float.IsFinite(velocity.Y))
    {
      return false;
    }

    behavior.State = behavior.State with { Primary = 1.0f, Secondary = 0.0f };
    velocity.X *= 0.5f;
    velocity.Y *= 0.5f;
    networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
    return true;
  }

  public void ApplyAcceptedHitDamage(
    ref ProjectileDamageComponent damage,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType == 357)
    {
      damage = new ProjectileDamageComponent((int)(damage.Amount * Type357DecayMultiplier));
      return;
    }

    if (definition.ProjectileType == 706)
    {
      damage = new ProjectileDamageComponent((int)(damage.Amount * 0.95d));
      return;
    }

    if (definition.ProjectileType == 876)
    {
      damage = new ProjectileDamageComponent((int)(damage.Amount * Type876DecayMultiplier));
      return;
    }

    if (definition.ProjectileType is not (638 or 639 or 640))
    {
      return;
    }

    damage = new ProjectileDamageComponent((int)(damage.Amount * 0.96d));
  }

  public void ApplyAcceptedHitKnockback(ref ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 876)
    {
      return;
    }

    definition = definition with { Knockback = definition.Knockback * Type876DecayMultiplier };
  }

  public bool ApplyAcceptedHitVelocity(
    ref VelocityComponent velocity,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 876 || !float.IsFinite(velocity.X) ||
        !float.IsFinite(velocity.Y))
    {
      return false;
    }

    velocity.X *= Type876VelocityMultiplier;
    velocity.Y *= Type876VelocityMultiplier;
    return true;
  }

  public bool ApplyAcceptedHitTileCollision(
    ref ProjectileBehaviorComponent behavior,
    ref ProjectileTileCollisionComponent tileCollision,
    ref ProjectileNetworkUpdateComponent networkUpdate,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 876)
    {
      return false;
    }

    behavior.State = behavior.State with
    {
      Secondary = tileCollision.Enabled ? 0.0f : 1.0f
    };
    networkUpdate = new ProjectileNetworkUpdatePolicy().RequestPrimaryUpdate(networkUpdate);
    return true;
  }

  public bool ApplyTileCollisionDamage(
    ref ProjectileDamageComponent damage,
    ref ProjectilePenetrationComponent penetration,
    ProjectileDefinitionComponent definition)
  {
    if (definition.ProjectileType != 357 || penetration.RemainingPenetration <= 0)
    {
      return false;
    }

    damage = new ProjectileDamageComponent((int)(damage.Amount * 0.9f), damage.HitCount);
    penetration.RemainingPenetration--;
    return true;
  }
}
