using System;
using System.Numerics;
using Terraria.Ai;
using Terraria.Entity;
using Terraria.Movement;

namespace Terraria.Projectile;

/// <summary>
/// Computes a homing velocity request from detached inputs. It never writes ECS state.
/// </summary>
public sealed class ProjectileHomingAiBehavior : AiBehavior<ProjectileAiIntent>
{
  private readonly ProjectileBehaviorProfile _profile;

  public ProjectileHomingAiBehavior(ProjectileBehaviorProfile profile)
      : base(profile?.Requirements ?? throw new ArgumentNullException(nameof(profile)))
  {
    if (profile.BehaviorId != new AiBehaviorId("HomingProjectile"))
    {
      throw new ArgumentException(
          "The profile does not describe the HomingProjectile behavior.", nameof(profile));
    }

    ValidateRequirements(profile.Requirements);
    _profile = profile;
  }

  protected override void CalculateCore(
      AiSnapshot snapshot,
      float deltaTime,
      AiOutput<ProjectileAiIntent> output)
  {
    if (!float.IsFinite(deltaTime) || deltaTime < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(deltaTime));
    }

    LocationComponent location = snapshot.Get<LocationComponent>();
    VelocityComponent velocity = snapshot.Get<VelocityComponent>();
    ProjectileHomingCapabilityComponent capability =
        snapshot.Get<ProjectileHomingCapabilityComponent>();
    ProjectileTargetFact target = snapshot.Get<ProjectileTargetFact>();
    if (!capability.Enabled || !target.IsUsable)
    {
      output.Emit(ProjectileAiIntent.Maintain(velocity.LinearVelocity));
      return;
    }

    Vector2 offset = target.Position - location.Coordinates;
    float rangeSquared = _profile.Range > 0f
        ? _profile.Range * _profile.Range
        : capability.Range * capability.Range;
    if (rangeSquared > 0f && offset.LengthSquared() > rangeSquared)
    {
      output.Emit(ProjectileAiIntent.Maintain(velocity.LinearVelocity));
      return;
    }

    float maximumSpeed = _profile.MaximumSpeed > 0f
        ? _profile.MaximumSpeed
        : velocity.LinearVelocity.Length();
    Vector2 desired = offset == Vector2.Zero
        ? velocity.LinearVelocity
        : Vector2.Normalize(offset) * maximumSpeed;
    float turnRate = _profile.TurnRate > 0f ? _profile.TurnRate : capability.TurnRate;
    Vector2 nextVelocity = MoveTowards(velocity.LinearVelocity, desired, turnRate * deltaTime);
    output.Emit(new ProjectileAiIntent(
        ProjectileAiIntentKind.AdjustVelocity, nextVelocity, target.Revision));
  }

  protected override void StopCore(AiOutput<ProjectileAiIntent> output)
  {
    output.Emit(new ProjectileAiIntent(
        ProjectileAiIntentKind.MaintainVelocity, Vector2.Zero));
  }

  private static void ValidateRequirements(AiRequirements requirements)
  {
    if (!requirements.Contains<LocationComponent>() ||
        !requirements.Contains<VelocityComponent>() ||
        !requirements.Contains<ProjectileHomingCapabilityComponent>() ||
        !requirements.Contains<ProjectileTargetFact>())
    {
      throw new ArgumentException(
          "The homing profile requirements do not provide the homing behavior inputs.",
          nameof(requirements));
    }
  }

  private static Vector2 MoveTowards(Vector2 current, Vector2 target, float distance)
  {
    Vector2 delta = target - current;
    float length = delta.Length();
    return length <= distance || length == 0f
        ? target
        : current + delta / length * distance;
  }
}
