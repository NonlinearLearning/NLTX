using System;
using System.Numerics;

using EntityEcs;
using EntityEcs.Components;

using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.Projectile;

/// <summary>
/// Owns the Version4 projectile pass boundary, ascending slot order, and
/// mutable numUpdates loop. Gameplay behavior remains delegated to the adapter.
/// </summary>
public sealed class ProjectileTickCoordinator
{
  public const int RegularSlotCount = 1000;

  private readonly ProjectileLifecycleSystem _lifecycle;
  private readonly EntityRuntime _runtime;

  public ProjectileTickCoordinator(ProjectileLifecycleSystem lifecycle)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    _lifecycle = lifecycle;
    _runtime = lifecycle.Runtime;
  }

  public ProjectileTickResult Tick(IProjectileTickAdapter adapter)
  {
    return TickCore(adapter, worldBounds: null);
  }

  public ProjectileTickResult Tick(
    IProjectileTickAdapter adapter,
    ProjectileWorldBounds worldBounds)
  {
    if (!worldBounds.IsValid)
    {
      throw new ArgumentException(
        "World bounds must be initialized by the validated constructor.",
        nameof(worldBounds));
    }

    return TickCore(adapter, worldBounds);
  }

  private ProjectileTickResult TickCore(
    IProjectileTickAdapter adapter,
    ProjectileWorldBounds? worldBounds)
  {
    ArgumentNullException.ThrowIfNull(adapter);

    int activeProjectileCount = 0;
    int skippedInactiveCount = 0;
    int updateStepCount = 0;
    int lifetimeExpiredCount = 0;

    adapter.PreUpdateAllProjectiles();
    bool hasDefenderEventSnapshot = adapter.TryGetDefenderEventOngoing(
      out bool isDefenderEventOngoing);
    for (int slotIndex = 0; slotIndex < RegularSlotCount; slotIndex++)
    {
      if (!_lifecycle.TryGetRuntimeHandleAtSlot(
        slotIndex,
        out ProjectileHandle handle,
        out RuntimeEntityHandle runtimeHandle))
      {
        continue;
      }

      ProjectileLifetimeStateComponent lifetime = Read<ProjectileLifetimeStateComponent>(
        runtimeHandle);
      if (!lifetime.Active)
      {
        skippedInactiveCount++;
        continue;
      }

      ProjectileHitImmunityPolicyComponent immunityPolicy =
        Read<ProjectileHitImmunityPolicyComponent>(runtimeHandle);
      ProjectileHitImmunityStateComponent immunity =
        Read<ProjectileHitImmunityStateComponent>(runtimeHandle);
      ProjectileHitImmunitySystem.AdvanceTick(ref immunity, in immunityPolicy);
      Write(runtimeHandle, immunity);

      ProjectileUpdateCadenceComponent cadence =
        Read<ProjectileUpdateCadenceComponent>(runtimeHandle);
      int initialSubstepCount = cadence.MaxUpdates;
      ProjectileTrajectoryStateComponent trajectory =
        Read<ProjectileTrajectoryStateComponent>(runtimeHandle);
      trajectory.NumUpdates = cadence.ExtraUpdates;
      Write(runtimeHandle, trajectory);

      activeProjectileCount++;
      int substepIndex = 0;
      while (true)
      {
        if (!TryGetActiveRuntimeHandle(
          handle,
          out RuntimeEntityHandle stepRuntimeHandle))
        {
          break;
        }

        trajectory = Read<ProjectileTrajectoryStateComponent>(stepRuntimeHandle);
        if (trajectory.NumUpdates < 0)
        {
          break;
        }

        trajectory.NumUpdates--;
        Write(stepRuntimeHandle, trajectory);

        var context = new ProjectileTickContext(
          handle,
          stepRuntimeHandle,
          slotIndex,
          substepIndex,
          initialSubstepCount,
          worldBounds);
        ProjectileTickPreparationInput preparationInput = CapturePreparationInput(
          stepRuntimeHandle);
        ProjectileTickPreparationResult preparationResult =
          adapter.PrepareProjectileStep(
            context,
            in preparationInput,
            out ProjectileTickPreparationChanges preparationChanges);
        updateStepCount++;
        substepIndex++;

        // Preparation is an adapter callback. Resolve the original handle again
        // before committing any of its bounded output or continuing this step.
        if (!TryGetActiveRuntimeHandle(
          handle,
          out RuntimeEntityHandle preparedRuntimeHandle) ||
          preparedRuntimeHandle != context.RuntimeHandle)
        {
          break;
        }

        ApplyPreparationChanges(preparedRuntimeHandle, in preparationChanges);

        if (preparationResult == ProjectileTickPreparationResult.Continued)
        {
          continue;
        }

        if (preparationResult == ProjectileTickPreparationResult.Returned)
        {
          break;
        }

        if (preparationResult ==
          ProjectileTickPreparationResult.WorldBoundaryDeactivation)
        {
          if (!_lifecycle.TryDeactivateAtWorldBoundary(handle))
          {
            throw new InvalidOperationException(
              "A world-boundary projectile could not be deactivated by the lifecycle owner.");
          }

          break;
        }

        if (preparationResult == ProjectileTickPreparationResult.IntegrationRequired)
        {
          throw new InvalidOperationException(
            "A minion or sentry update requires a Player owner adapter.");
        }

        if (preparationResult != ProjectileTickPreparationResult.Ready)
        {
          throw new InvalidOperationException(
            "The projectile adapter returned an unknown preparation result.");
        }

        ProjectileEffectCooldownStateComponent effectCooldown =
          Read<ProjectileEffectCooldownStateComponent>(preparedRuntimeHandle);
        ProjectileEffectCooldownSystem.Advance(ref effectCooldown);
        Write(preparedRuntimeHandle, effectCooldown);

        ProjectileNetworkStateComponent network =
          Read<ProjectileNetworkStateComponent>(preparedRuntimeHandle);
        ProjectileNetworkStateSystem.BeginProjectileUpdateSubstep(ref network);
        Write(preparedRuntimeHandle, network);

        ProjectileTickSubstepResult substepResult = adapter.UpdateProjectileStep(context);

        if (substepResult == ProjectileTickSubstepResult.Continued)
        {
          continue;
        }

        if (substepResult == ProjectileTickSubstepResult.Returned)
        {
          break;
        }

        if (substepResult != ProjectileTickSubstepResult.Completed)
        {
          throw new InvalidOperationException(
            "The projectile adapter returned an unknown substep result.");
        }

        if (!RecordTrailHistory(adapter, context))
        {
          break;
        }

        // Update, owner movement, and presentation callbacks can terminate the
        // instance or replace its slot. Only the original root reaches the tail.
        if (!TryGetActiveRuntimeHandle(
          handle,
          out RuntimeEntityHandle currentRuntimeHandle) ||
          currentRuntimeHandle != context.RuntimeHandle)
        {
          break;
        }

        lifetime = Read<ProjectileLifetimeStateComponent>(currentRuntimeHandle);
        ProjectileDefinitionComponent definition =
          Read<ProjectileDefinitionComponent>(currentRuntimeHandle);
        if (ProjectileSpecializedDefinitionQuery.IsAdd2Turret(
          definition.ProjectileType))
        {
          if (!hasDefenderEventSnapshot)
          {
            throw new InvalidOperationException(
              "An IsADD2Turret projectile requires an Old One's Army event snapshot.");
          }

          if (isDefenderEventOngoing)
          {
            lifetime.TimeLeft++;
          }
        }

        if (ProjectileLifetimeSystem.Advance(ref lifetime))
        {
          if (!_lifecycle.TryTerminate(
            handle,
            ProjectileEndReason.LifetimeExpired))
          {
            throw new InvalidOperationException(
              "An expired projectile could not be released by the lifecycle owner.");
          }

          lifetimeExpiredCount++;
          break;
        }

        Write(currentRuntimeHandle, lifetime);
        ProjectilePenetrationStateComponent penetration =
          Read<ProjectilePenetrationStateComponent>(currentRuntimeHandle);
        if (penetration.RemainingHits == 0)
        {
          if (!_lifecycle.TryTerminate(
            handle,
            ProjectileEndReason.HitLimitReached))
          {
            throw new InvalidOperationException(
              "A depleted projectile could not be released by the lifecycle owner.");
          }

          break;
        }
      }
    }

    adapter.PostUpdateAllProjectiles();
    return new ProjectileTickResult(
      activeProjectileCount,
      skippedInactiveCount,
      updateStepCount)
    {
      LifetimeExpiredCount = lifetimeExpiredCount,
    };
  }

  private bool TryGetActiveRuntimeHandle(
    ProjectileHandle handle,
    out RuntimeEntityHandle runtimeHandle)
  {
    if (!_lifecycle.TryGetRuntimeHandle(handle, out runtimeHandle) ||
      !Read<ProjectileLifetimeStateComponent>(runtimeHandle).Active)
    {
      runtimeHandle = default;
      return false;
    }

    return true;
  }

  private ProjectileTickPreparationInput CapturePreparationInput(
    RuntimeEntityHandle runtimeHandle)
  {
    bool hasMinion = _runtime.Has<ProjectileMinionCapabilityComponent>(runtimeHandle);
    ProjectileMinionCapabilityComponent minion = hasMinion
      ? Read<ProjectileMinionCapabilityComponent>(runtimeHandle)
      : default;
    bool hasSentry = _runtime.Has<ProjectileSentryCapabilityComponent>(runtimeHandle);
    ProjectileSentryCapabilityComponent sentry = hasSentry
      ? Read<ProjectileSentryCapabilityComponent>(runtimeHandle)
      : default;
    return new ProjectileTickPreparationInput(
      Read<ProjectileDefinitionComponent>(runtimeHandle),
      Read<ProjectileBehaviorStateComponent>(runtimeHandle),
      Read<ProjectileTrajectoryStateComponent>(runtimeHandle),
      Read<LocationComponent>(runtimeHandle),
      Read<VelocityComponent>(runtimeHandle),
      Read<ColliderComponent>(runtimeHandle),
      hasMinion,
      minion,
      hasSentry,
      sentry);
  }

  private void ApplyPreparationChanges(
    RuntimeEntityHandle runtimeHandle,
    in ProjectileTickPreparationChanges changes)
  {
    if (changes.BehaviorAi1 is float ai1 &&
      !_runtime.TryEdit<ProjectileBehaviorStateComponent>(
        runtimeHandle,
        (ref ProjectileBehaviorStateComponent behavior) => behavior.Ai1 = ai1))
    {
      ThrowMissingComponent<ProjectileBehaviorStateComponent>();
    }

    if (changes.TrajectoryGfxOffY is float gfxOffY &&
      !_runtime.TryEdit<ProjectileTrajectoryStateComponent>(
        runtimeHandle,
        (ref ProjectileTrajectoryStateComponent trajectory) =>
          trajectory.GfxOffY = gfxOffY))
    {
      ThrowMissingComponent<ProjectileTrajectoryStateComponent>();
    }

    if (changes.PreviousVelocity is VelocityComponent previousVelocity &&
      !_runtime.TryEdit<MotionHistoryComponent>(
        runtimeHandle,
        (ref MotionHistoryComponent history) =>
          history.PreviousVelocity = previousVelocity))
    {
      ThrowMissingComponent<MotionHistoryComponent>();
    }
  }

  private bool RecordTrailHistory(
    IProjectileTickAdapter adapter,
    ProjectileTickContext context)
  {
    if (!TryGetActiveRuntimeHandle(context.Handle, out RuntimeEntityHandle runtimeHandle) ||
      runtimeHandle != context.RuntimeHandle)
    {
      return false;
    }

    ProjectilePresentationStateComponent presentation =
      Read<ProjectilePresentationStateComponent>(runtimeHandle);
    int trailingMode = presentation.TrailingMode;
    Vector2? ownerMovementDelta = null;
    if (trailingMode == 4)
    {
      ProjectileIdentityComponent identity = Read<ProjectileIdentityComponent>(runtimeHandle);
      if (!adapter.TryGetOwnerMovementDelta(
        context,
        identity.OwnerReference,
        out Vector2 movementDelta))
      {
        throw new InvalidOperationException(
          "Trailing mode 4 requires a Player owner movement snapshot.");
      }

      if (!TryGetActiveRuntimeHandle(context.Handle, out runtimeHandle) ||
        runtimeHandle != context.RuntimeHandle)
      {
        return false;
      }

      ownerMovementDelta = movementDelta;
    }

    ProjectileAnimationStateComponent animation =
      Read<ProjectileAnimationStateComponent>(runtimeHandle);
    LocationComponent location = Read<LocationComponent>(runtimeHandle);
    VelocityComponent velocity = Read<VelocityComponent>(runtimeHandle);
    ProjectileTrajectoryStateComponent trajectory =
      Read<ProjectileTrajectoryStateComponent>(runtimeHandle);
    ProjectileDefinitionComponent definition =
      Read<ProjectileDefinitionComponent>(runtimeHandle);
    ProjectileTrailDustRequest? dustRequest = null;
    if (!_runtime.TryEdit<ProjectileTrailCacheComponent>(
      runtimeHandle,
      (ref ProjectileTrailCacheComponent trail) =>
        dustRequest = ProjectileTrailCacheSystem.Record(
          ref trail,
          trailingMode,
          animation.FrameCounter,
          new Vector2(location.X, location.Y),
          trajectory.Rotation,
          trajectory.SpriteDirection,
          new Vector2(velocity.X, velocity.Y),
          trajectory.NumUpdates,
          ownerMovementDelta,
          definition.ProjectileType)))
    {
      ThrowMissingComponent<ProjectileTrailCacheComponent>();
    }

    if (dustRequest.HasValue)
    {
      adapter.EmitTrailDust(context, dustRequest.Value);
      return TryGetActiveRuntimeHandle(
          context.Handle,
          out RuntimeEntityHandle afterDustRuntimeHandle) &&
        afterDustRuntimeHandle == context.RuntimeHandle;
    }

    return true;
  }

  private TComponent Read<TComponent>(RuntimeEntityHandle runtimeHandle)
    where TComponent : struct
  {
    TComponent value = default;
    if (!_runtime.TryInspect(
      runtimeHandle,
      (in TComponent component) => value = component))
    {
      throw new InvalidOperationException(
        $"The projectile root does not expose {typeof(TComponent).Name}.");
    }

    return value;
  }

  private void Write<TComponent>(
    RuntimeEntityHandle runtimeHandle,
    TComponent value)
    where TComponent : struct
  {
    if (!_runtime.TryReplace(runtimeHandle, value))
    {
      ThrowMissingComponent<TComponent>();
    }
  }

  private static void ThrowMissingComponent<TComponent>()
  {
    throw new InvalidOperationException(
      $"The projectile {typeof(TComponent).Name} component could not be accessed.");
  }
}
