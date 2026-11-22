using System;
using Arch.Core;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Attributes;
using Terraria.Combat;
using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>
/// Applies accepted combat receipts to projectile-owned penetration and immunity state.
/// It never writes target life and leaves entity destruction to the lifecycle owner.
/// </summary>
public sealed class ProjectileHitResolutionSystem
{
  private readonly ArchWorld _world;
  private readonly IEntityRuntimeResolver _resolver;
  private ProjectileTerminationQueueComponent? _terminations;

  public ProjectileHitResolutionSystem(
      ArchWorld world,
      IEntityRuntimeResolver resolver)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
  }

  /// <summary>
  /// World-scoped termination queue, resolved on first use so construction order never decides
  /// which instance a system observes.
  /// </summary>
  private ProjectileTerminationQueueComponent Terminations =>
      _terminations ??= ProjectileTerminationQueue.Require(_world);

  public ProjectileHitAdmissionResult CheckAdmission(
      EntityRuntimeHandle projectile,
      EntityRuntimeHandle target,
      long tick)
  {
    if (!_resolver.TryResolve(projectile, out ArchEntity entity) ||
        entity == ArchEntity.Null || !_world.IsAlive(entity))
    {
      return ProjectileHitAdmissionResult.ProjectileUnavailable;
    }

    if (!_world.Has<ProjectileDispositionStateComponent>(entity))
    {
      return ProjectileHitAdmissionResult.ProjectileUnavailable;
    }

    if (Terminations.Contains(projectile, tick))
    {
      return ProjectileHitAdmissionResult.ProjectileInactive;
    }

    ProjectileDispositionStateComponent disposition =
        _world.Get<ProjectileDispositionStateComponent>(entity);
    if (!disposition.IsActive)
    {
      return ProjectileHitAdmissionResult.ProjectileInactive;
    }

    if (_world.Has<ProjectileHitImmunityStateComponent>(entity))
    {
      ProjectileHitImmunityStateComponent immunity =
          _world.Get<ProjectileHitImmunityStateComponent>(entity);
      immunity.Prune(tick);
      if (immunity.AppliesTo(target, tick))
      {
        return ProjectileHitAdmissionResult.TargetImmune;
      }
    }

    return ProjectileHitAdmissionResult.Accepted;
  }

  /// <summary>Consumes a shared receipt produced for a Projectile damage request.</summary>
  public ProjectileHitAdmissionResult Apply(
      in DamageAppliedReceipt receipt,
      out ProjectileTerminationReason? terminationReason)
  {
    return ApplyCore(
        receipt.Source,
        receipt.Target,
        receipt.Tick,
        out terminationReason);
  }

  private ProjectileHitAdmissionResult ApplyCore(
      EntityRuntimeHandle projectileHandle,
      EntityRuntimeHandle targetHandle,
      long tick,
      out ProjectileTerminationReason? terminationReason)
  {
    terminationReason = null;
    ProjectileHitAdmissionResult admission = CheckAdmission(
        projectileHandle,
        targetHandle,
        tick);
    if (admission != ProjectileHitAdmissionResult.Accepted)
    {
      return admission;
    }

    if (!_resolver.TryResolve(projectileHandle, out ArchEntity entity) ||
        entity == ArchEntity.Null || !_world.IsAlive(entity) ||
        !_world.Has<ProjectileDispositionStateComponent>(entity))
    {
      return ProjectileHitAdmissionResult.ProjectileUnavailable;
    }

    if (!_world.Get<ProjectileDispositionStateComponent>(entity).IsActive)
    {
      return ProjectileHitAdmissionResult.ProjectileInactive;
    }

    // Immunity is checked before Combat admission. Once Combat returns an accepted receipt,
    // this method records that receipt and must not reject it after Life has already changed.
    AttributeSetComponent? attributes = _world.Has<AttributeSetComponent>(entity)
        ? _world.Get<AttributeSetComponent>(entity)
        : null;
    float effectiveImmunityTicksValue = attributes is null
        ? 0f
        : AttributeCalculator.Resolve(
            attributes,
            AttributeKind.ProjectileHitImmunityTicks);
    long effectiveImmunityTicks = ResolveTicks(
        effectiveImmunityTicksValue,
        nameof(effectiveImmunityTicksValue));
    if (_world.Has<ProjectileHitImmunityStateComponent>(entity))
    {
      ProjectileHitImmunityStateComponent immunity =
          _world.Get<ProjectileHitImmunityStateComponent>(entity);
      immunity.Record(
          targetHandle,
          tick,
          checked(tick + effectiveImmunityTicks));
    }

    if (_world.Has<ProjectilePenetrationStateComponent>(entity))
    {
      ref ProjectilePenetrationStateComponent penetration =
          ref _world.Get<ProjectilePenetrationStateComponent>(entity);
      penetration = penetration.RecordAcceptedHit();
      float penetrationLimit = attributes is null
          ? -1f
          : AttributeCalculator.Resolve(
              attributes,
              AttributeKind.ProjectilePenetrationLimit,
              fallback: -1f);
      if (penetration.HasReachedLimit(penetrationLimit))
      {
        terminationReason = ProjectileTerminationReason.HitLimitReached;
        Terminations.Enqueue(
            projectileHandle,
            ProjectileTerminationReason.HitLimitReached,
            tick);      }
    }

    return ProjectileHitAdmissionResult.Accepted;
  }

  private static long ResolveTicks(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0f || value > long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }

    return checked((long)MathF.Ceiling(value));
  }
}
