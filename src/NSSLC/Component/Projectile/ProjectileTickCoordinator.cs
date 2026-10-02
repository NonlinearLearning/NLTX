using System;

using Terraria.WorldStorage;

namespace Terraria.Projectile;

/// <summary>
/// Owns the Version4 projectile pass boundary and ascending slot order.
/// Gameplay behavior remains delegated to the supplied adapter.
/// </summary>
public sealed class ProjectileTickCoordinator
{
  public const int RegularSlotCount = 1000;

  private readonly ProjectileLifecycleSystem _lifecycle;

  public ProjectileTickCoordinator(ProjectileLifecycleSystem lifecycle)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    _lifecycle = lifecycle;
  }

  public ProjectileTickResult Tick(IProjectileTickAdapter adapter)
  {
    ArgumentNullException.ThrowIfNull(adapter);

    int activeProjectileCount = 0;
    int skippedInactiveCount = 0;
    int updateStepCount = 0;

    adapter.PreUpdateAllProjectiles();
    for (int slotIndex = 0; slotIndex < RegularSlotCount; slotIndex++)
    {
      if (!_lifecycle.TryGetAtSlot(
        slotIndex,
        out ProjectileHandle handle,
        out ProjectileEntityState? state) ||
        state is null)
      {
        continue;
      }

      if (!state.Lifetime.Active)
      {
        skippedInactiveCount++;
        continue;
      }

      ProjectileHitImmunityStateComponent immunity = state.HitImmunity;
      ProjectileHitImmunitySystem.AdvanceTick(
        ref immunity,
        state.HitImmunityPolicy);
      state.HitImmunity = immunity;

      int substepCount = state.UpdateCadence.MaxUpdates;
      if (substepCount <= 0)
      {
        throw new InvalidOperationException(
          "Projectile update cadence must provide at least one substep.");
      }

      activeProjectileCount++;
      for (int substepIndex = 0; substepIndex < substepCount; substepIndex++)
      {
        if (!_lifecycle.TryGet(
          handle,
          out ProjectileEntityState? currentSubstepState) ||
          currentSubstepState is null ||
          !currentSubstepState.Lifetime.Active)
        {
          break;
        }

        adapter.UpdateProjectile(
          new ProjectileTickContext(
            handle,
            slotIndex,
            substepIndex,
            substepCount),
          currentSubstepState);
        updateStepCount++;
      }

      if (!_lifecycle.TryGet(
        handle,
        out ProjectileEntityState? currentState) ||
        currentState is null ||
        !currentState.Lifetime.Active)
      {
        continue;
      }

      ProjectileLifetimeStateComponent lifetime = currentState.Lifetime;
      if (ProjectileLifetimeSystem.Advance(ref lifetime))
      {
        if (!_lifecycle.TryTerminate(
          handle,
          ProjectileEndReason.LifetimeExpired))
        {
          throw new InvalidOperationException(
            "An expired projectile could not be released by the lifecycle owner.");
        }
      }
      else
      {
        currentState.Lifetime = lifetime;
      }
    }

    adapter.PostUpdateAllProjectiles();
    return new ProjectileTickResult(
      activeProjectileCount,
      skippedInactiveCount,
      updateStepCount);
  }
}
