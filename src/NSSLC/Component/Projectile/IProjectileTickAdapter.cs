using System;
using System.Numerics;

using Terraria.Relationships;

namespace Terraria.Projectile;

/// <summary>
/// Receives the ordered lifecycle shell without owning slot allocation.
/// Implementations may delegate AI, movement, collision, combat, or
/// presentation work to their respective Systems.
/// </summary>
public interface IProjectileTickAdapter
{
  void PreUpdateAllProjectiles();

  /// <summary>
  /// Runs the legacy substep prefix through the point immediately before the
  /// shared sound-delay decrement. Early continue/return paths skip that
  /// decrement and the remaining substep. The default applies the confirmed
  /// prefix rules. Overrides should call
  /// <see cref="ProjectileTickPreparationSystem.PrepareProjectileStep"/> before
  /// handling additional Version4 prefix behavior.
  /// </summary>
  ProjectileTickPreparationResult PrepareProjectileStep(
    ProjectileTickContext context,
    in ProjectileTickPreparationInput input,
    out ProjectileTickPreparationChanges changes)
  {
    return ProjectileTickPreparationSystem.PrepareProjectileStep(
      context,
      in input,
      out changes);
  }

  /// <summary>
  /// Runs projectile behavior after shared pre-AI effects and before the
  /// coordinator-owned trail and lifetime tail.
  /// </summary>
  void UpdateProjectile(
    ProjectileTickContext context);

  /// <summary>
  /// Runs projectile behavior and reports whether it reached the coordinator's
  /// trail and lifetime tail, continued early, or returned from the projectile
  /// update. Legacy adapters inherit a normal-completion result.
  /// </summary>
  ProjectileTickSubstepResult UpdateProjectileStep(
    ProjectileTickContext context)
  {
    UpdateProjectile(context);
    return ProjectileTickSubstepResult.Completed;
  }

  /// <summary>
  /// Captures Main.player[owner].position - oldPosition for trailing mode 4.
  /// Other trail modes do not request this Player-owned input.
  /// </summary>
  bool TryGetOwnerMovementDelta(
    ProjectileTickContext context,
    EntityReference ownerReference,
    out Vector2 movementDelta)
  {
    movementDelta = default;
    return false;
  }

  /// <summary>
  /// Executes the source-defined mode 1 trail dust effect at the point where
  /// trail history has been recorded. Adapters without a particle owner fail
  /// explicitly when this effect is requested.
  /// </summary>
  void EmitTrailDust(
    ProjectileTickContext context,
    ProjectileTrailDustRequest request)
  {
    throw new InvalidOperationException(
      "Projectile trail dust requires a presentation effects adapter.");
  }

  /// <summary>
  /// Captures whether the Old One's Army event is ongoing. The coordinator
  /// calls this once after PreUpdateAllProjectiles for each pass and uses the
  /// result for every IsADD2Turret projectile in that pass.
  /// Implementations should capture the current read-only world value without
  /// changing event state.
  /// </summary>
  bool TryGetDefenderEventOngoing(out bool isOngoing)
  {
    isOngoing = false;
    return false;
  }

  void PostUpdateAllProjectiles();
}
