using Terraria.WorldStorage;

namespace Terraria.Projectile;

public sealed class ProjectileEntityState : WorldEntityState
{
  public ProjectileDefinitionComponent Definition { get; internal set; }

  public ProjectileIdentityComponent Identity { get; internal set; }

  public ProjectileLifetimeStateComponent Lifetime { get; internal set; }

  public ProjectileNetworkStateComponent Network { get; internal set; }

  public ProjectileGeometryStateComponent Geometry { get; internal set; }

  public ProjectileBehaviorStateComponent Behavior { get; internal set; }

  public ProjectileUpdateCadenceComponent UpdateCadence { get; internal set; }

  public ProjectileDispositionStateComponent Disposition { get; internal set; }

  public ProjectileDamagePayloadComponent Damage { get; internal set; }

  public ProjectileDamagePolicyComponent DamagePolicy { get; internal set; }

  public ProjectilePenetrationStateComponent Penetration { get; internal set; }

  public ProjectileCollisionPolicyComponent Collision { get; internal set; }

  public ProjectileHitImmunityPolicyComponent HitImmunityPolicy { get; internal set; }

  public ProjectileHitImmunityStateComponent HitImmunity { get; internal set; }

  public ProjectilePresentationStateComponent Presentation { get; internal set; }

  public ProjectileAnimationStateComponent Animation { get; internal set; }

  public ProjectileTrailCacheComponent Trail { get; internal set; }

  public ProjectileSourceMetadataComponent Source { get; internal set; }

  public ProjectileMinionCapabilityComponent Minion { get; internal set; }

  public ProjectileSentryCapabilityComponent Sentry { get; internal set; }

  public ProjectileBobberCapabilityComponent Bobber { get; internal set; }

  public ProjectileCounterweightCapabilityComponent Counterweight { get; internal set; }

  public ProjectileTrapCapabilityComponent Trap { get; internal set; }

  public ProjectileKinematicsStateComponent Kinematics { get; internal set; }

  public ProjectileEntityState(
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
    Disposition = default;
    Damage = default;
    DamagePolicy = default;
    Penetration = default;
    Collision = default;
    HitImmunityPolicy = default;
    HitImmunity = new ProjectileHitImmunityStateComponent(
      npcCapacity: 0,
      playerCapacity: Network.SectionSyncSkippedForPlayer.Length);
    Presentation = default;
    Animation = default;
    Trail = new ProjectileTrailCacheComponent();
    Source = new ProjectileSourceMetadataComponent();
    Minion = default;
    Sentry = default;
    Bobber = default;
    Counterweight = default;
    Trap = default;
    Kinematics = default;
  }
}
