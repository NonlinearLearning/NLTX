using System.Numerics;

namespace Terraria.Projectile;

public struct ProjectileDirectionComponent
{
  public ProjectileDirectionComponent(Vector2 trajectory)
  {
    Trajectory = trajectory;
  }

  public Vector2 Trajectory;
}
