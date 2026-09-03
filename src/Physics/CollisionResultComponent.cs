namespace Terraria.Physics;

public struct CollisionResultComponent
{
  public CollisionResultComponent(bool collidedOnX, bool collidedOnY)
  {
    CollidedOnX = collidedOnX;
    CollidedOnY = collidedOnY;
  }

  public bool CollidedOnX;
  public bool CollidedOnY;
}
