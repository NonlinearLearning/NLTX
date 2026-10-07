namespace Terraria.Projectile;

/// <summary>
/// Maps Version4 Damage_GetHitbox, including its LocalAi0 write for expanding
/// projectiles of types 301, 383, and 262.
/// </summary>
public static class ProjectileDamageHitboxSystem
{
  /// <param name="isPhaseblade">
  /// Snapshot of Version4 ProjectileID.Sets.IsAPhaseblade[type].
  /// </param>
  public static ProjectileDamageHitbox Build(
    in ProjectileCollisionGeometryInput input,
    bool isPhaseblade,
    out ProjectileBehaviorStateComponent updatedBehavior)
  {
    ProjectileKinematicsStateComponent kinematics = input.Kinematics;
    ProjectileBehaviorStateComponent behavior = input.Behavior;
    int projectileType = input.Definition.ProjectileType;
    int aiStyle = input.Definition.BehaviorKey;

    var hitbox = new ProjectileDamageHitbox(
      (int)kinematics.Position.X,
      (int)kinematics.Position.Y,
      input.Width,
      input.Height);

    if (isPhaseblade && behavior.Ai0 == 2.0f)
    {
      Inflate(ref hitbox, -14, 0);
    }

    if ((projectileType is 301 or 383 or 262) && behavior.LocalAi0 > 0.0f)
    {
      Inflate(ref hitbox, -input.Width / 2, -input.Height / 2);
      int localRadius = (int)(behavior.LocalAi0 / 2.0f);
      int verticalRadius = (int)behavior.LocalAi0 / 2;
      Inflate(ref hitbox, localRadius, verticalRadius);
      behavior.LocalAi0 = -1.0f;
    }

    if (projectileType == 101)
    {
      Inflate(ref hitbox, 30, 30);
    }

    if (projectileType == 1024)
    {
      Inflate(ref hitbox, 6, 6);
    }

    if (projectileType == 1023)
    {
      Inflate(ref hitbox, 8, 8);
    }

    if (projectileType == 85)
    {
      int radius = (int)Remap(behavior.LocalAi0, 10.0f, 40.0f);
      Inflate(ref hitbox, radius, radius);
    }

    if (projectileType == 1106)
    {
      int radius = (int)Remap(behavior.LocalAi0, 0.0f, 20.0f);
      Inflate(ref hitbox, radius, radius);
    }

    if (projectileType == 188)
    {
      Inflate(ref hitbox, 20, 20);
    }

    if (aiStyle == 29)
    {
      Inflate(ref hitbox, 4, 4);
    }

    if (projectileType == 967)
    {
      Inflate(ref hitbox, 10, 10);
    }

    updatedBehavior = behavior;
    return hitbox;
  }

  private static void Inflate(
    ref ProjectileDamageHitbox hitbox,
    int horizontal,
    int vertical)
  {
    hitbox = hitbox with
    {
      X = hitbox.X - horizontal,
      Y = hitbox.Y - vertical,
      Width = hitbox.Width + horizontal * 2,
      Height = hitbox.Height + vertical * 2,
    };
  }

  private static float Remap(
    float value,
    float outputMinimum,
    float outputMaximum)
  {
    float amount = Math.Clamp(value / 72.0f, 0.0f, 1.0f);
    return outputMinimum + (outputMaximum - outputMinimum) * amount;
  }
}
