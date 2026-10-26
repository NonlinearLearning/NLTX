using System;
using System.Numerics;
using Terraria.Ai;
using Terraria.Entity;
using Terraria.Movement;

namespace Terraria.Projectile;

/// <summary>Emits the current velocity as a deterministic straight-flight request.</summary>
public sealed class ProjectileStraightAiBehavior : AiBehavior<ProjectileAiIntent>
{
  public ProjectileStraightAiBehavior()
      : base(CreateRequirements())
  {
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

    VelocityComponent velocity = snapshot.Get<VelocityComponent>();
    output.Emit(ProjectileAiIntent.Maintain(velocity.LinearVelocity));
  }

  protected override void StopCore(AiOutput<ProjectileAiIntent> output)
  {
    output.Emit(new ProjectileAiIntent(
        ProjectileAiIntentKind.MaintainVelocity, Vector2.Zero));
  }

  private static AiRequirements CreateRequirements()
  {
    return AiRequirements.Empty.Require<VelocityComponent>();
  }
}
