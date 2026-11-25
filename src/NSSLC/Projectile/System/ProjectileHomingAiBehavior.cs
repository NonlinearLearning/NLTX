using System;
using System.Numerics;
using Terraria.Share.Ai;
using Terraria.Share.Entities;
using Terraria.Share.Movement;

namespace Terraria.Unshared.Projectiles;

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
    Vector2 nextVelocity = Terraria.Share.Physics.SimulationVector.MoveTowards(
        velocity.LinearVelocity, desired, turnRate * deltaTime);
    // 转向不得掉速：MoveTowards 走的是弦，而"目标速度"又按当前速度重算，速率会像滚雪球一样衰减
    // （实测 40 tick 从 8 掉到 3.84）。参考端追踪弹保持恒定速率、只改方向，因此把速率钉回原值。
    float speed = velocity.LinearVelocity.Length();
    if (speed > 0f && nextVelocity != Vector2.Zero)
    {
      nextVelocity = Vector2.Normalize(nextVelocity) * speed;
    }

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

}
