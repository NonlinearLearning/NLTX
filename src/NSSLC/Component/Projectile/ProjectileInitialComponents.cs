using EntityEcs.Components;

namespace Terraria.Projectile;

/// <summary>
/// Temporary component composition produced by hydration and consumed by the
/// projectile lifecycle owner before the entity is published.
/// </summary>
internal sealed class ProjectileInitialComponents
{
  public ProjectileDefinitionComponent Definition { get; set; }

  public ProjectileIdentityComponent Identity { get; set; }

  public ProjectileLifetimeStateComponent Lifetime { get; set; }

  public ProjectileNetworkStateComponent Network { get; set; }

  public ProjectileGeometryStateComponent Geometry { get; set; }

  public ProjectileBehaviorStateComponent Behavior { get; set; }

  public ProjectileUpdateCadenceComponent UpdateCadence { get; set; }

  public ProjectileTrajectoryStateComponent Trajectory { get; set; }

  public DirectionComponent Direction { get; set; }

  public ProjectileDispositionStateComponent Disposition { get; set; }

  public ProjectileDamagePayloadComponent Damage { get; set; }

  public ProjectileDamagePolicyComponent DamagePolicy { get; set; }

  public ProjectilePenetrationStateComponent Penetration { get; set; }

  public ProjectileCollisionPolicyComponent Collision { get; set; }

  public ProjectileWetStateComponent WetState { get; set; }

  public ProjectileHitImmunityPolicyComponent HitImmunityPolicy { get; set; }

  public ProjectileHitImmunityStateComponent HitImmunity { get; set; }

  public ProjectileEffectCooldownStateComponent EffectCooldown { get; set; }

  public ProjectileReflectionStateComponent Reflection { get; set; }

  public ProjectilePresentationStateComponent Presentation { get; set; }

  public ProjectileAnimationStateComponent Animation { get; set; }

  public ProjectileTrailCacheComponent Trail { get; set; }

  public ProjectileSourceMetadataComponent Source { get; set; }

  public ProjectileMinionCapabilityComponent Minion { get; set; }

  public ProjectileSentryCapabilityComponent Sentry { get; set; }

  public ProjectileBobberCapabilityComponent Bobber { get; set; }

  public ProjectileCounterweightCapabilityComponent Counterweight { get; set; }

  public ProjectileTrapCapabilityComponent Trap { get; set; }

  public ProjectileKinematicsStateComponent Kinematics { get; set; }

  public MotionHistoryComponent MotionHistory { get; set; }

  public ProjectileInitialComponents(
    ProjectileIdentityComponent identity,
    ProjectileLifetimeStateComponent lifetime,
    ProjectileNetworkStateComponent network)
  {
    Definition = default;
    Identity = identity;
    Lifetime = lifetime;
    Network = network.SectionSyncSkippedForPlayer is null
      ? new ProjectileNetworkStateComponent()
      : network;
    Geometry = default;
    Behavior = new ProjectileBehaviorStateComponent();
    UpdateCadence = default;
    Trajectory = new ProjectileTrajectoryStateComponent(
      rotation: 0.0f,
      spriteDirection: 1,
      stepSpeed: 1.0f,
      numUpdates: 0,
      gfxOffY: 0.0f);
    Direction = new DirectionComponent(horizontal: 1);
    Disposition = default;
    Damage = default;
    DamagePolicy = default;
    Penetration = new ProjectilePenetrationStateComponent(
      remainingHits: 1,
      maximumHits: 1);
    Collision = default;
    WetState = default;
    HitImmunityPolicy = default;
    HitImmunity = new ProjectileHitImmunityStateComponent(
      npcCapacity: 0,
      playerCapacity: Network.SectionSyncSkippedForPlayer.Length);
    EffectCooldown = default;
    Reflection = default;
    Presentation = new ProjectilePresentationStateComponent(trailingMode: -1);
    Animation = default;
    Trail = new ProjectileTrailCacheComponent();
    Source = new ProjectileSourceMetadataComponent();
    Minion = default;
    Sentry = default;
    Bobber = default;
    Counterweight = default;
    Trap = default;
    Kinematics = default;
    MotionHistory = default;
  }
}
