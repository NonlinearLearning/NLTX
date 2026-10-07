using EntityEcs.Components;

namespace Terraria.Projectile;

/// <summary>
/// Applies a confirmed early-out from the Version4 projectile update prefix.
/// </summary>
public static class ProjectileTickPreparationSystem
{
  /// <summary>
  /// Applies the confirmed update-prefix rules in source order and stops when
  /// a lifecycle or Player owner must handle the projectile.
  /// </summary>
  public static ProjectileTickPreparationResult PrepareProjectileStep(
    ProjectileTickContext context,
    in ProjectileTickPreparationInput input,
    out ProjectileTickPreparationChanges changes)
  {
    changes = default;
    ProjectileDefinitionComponent definition = input.Definition;
    float initialAi1 = input.Behavior.Ai1;
    ProjectileTickPreparationResult countdownResult = PrepareType640Countdown(
      definition,
      initialAi1,
      out float updatedAi1);
    if (countdownResult != ProjectileTickPreparationResult.Ready)
    {
      changes = new ProjectileTickPreparationChanges(behaviorAi1: updatedAi1);
      return countdownResult;
    }

    ProjectileTickPreparationResult boundaryResult = PrepareWorldBoundary(
      context,
      in input);
    if (boundaryResult != ProjectileTickPreparationResult.Ready)
    {
      return boundaryResult;
    }

    if (input.Trajectory.NumUpdates == -1 &&
      ((input.HasMinionCapability && input.Minion.IsMinion) ||
       (input.HasSentryCapability && input.Sentry.IsSentry)))
    {
      return ProjectileTickPreparationResult.IntegrationRequired;
    }

    ProjectileTrajectoryStateComponent trajectory = input.Trajectory;
    ProjectileMotionAndAiSystem.AdvanceGfxOffY(
      ref trajectory,
      input.Velocity.X);
    changes = new ProjectileTickPreparationChanges(
      trajectoryGfxOffY: trajectory.GfxOffY,
      previousVelocity: input.Velocity);
    return ProjectileTickPreparationResult.Ready;
  }

  /// <summary>
  /// Applies the type-640 countdown branch before the shared sound-delay decrement.
  /// </summary>
  public static ProjectileTickPreparationResult PrepareType640Countdown(
    ProjectileDefinitionComponent definition,
    float ai1,
    out float updatedAi1)
  {
    updatedAi1 = ai1;
    if (definition.ProjectileType != 640 || ai1 <= 0.0f)
    {
      return ProjectileTickPreparationResult.Ready;
    }

    updatedAi1 -= 1.0f;
    return ProjectileTickPreparationResult.Continued;
  }

  /// <summary>
  /// Applies the non-minion world-edge early deactivation when the caller
  /// supplied a world snapshot. Minions require a Player owner adapter.
  /// </summary>
  public static ProjectileTickPreparationResult PrepareWorldBoundary(
    ProjectileTickContext context,
    in ProjectileTickPreparationInput input)
  {
    if (context.WorldBounds is not ProjectileWorldBounds worldBounds ||
      input.Definition.BehaviorKey == 3 ||
      worldBounds.ContainsProjectile(
        new System.Numerics.Vector2(input.Location.X, input.Location.Y),
        input.Collider.Width,
        input.Collider.Height))
    {
      return ProjectileTickPreparationResult.Ready;
    }

    return input.HasMinionCapability && input.Minion.IsMinion
      ? ProjectileTickPreparationResult.IntegrationRequired
      : ProjectileTickPreparationResult.WorldBoundaryDeactivation;
  }
}
