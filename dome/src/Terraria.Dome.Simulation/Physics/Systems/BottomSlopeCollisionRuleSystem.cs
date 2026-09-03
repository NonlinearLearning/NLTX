namespace Terraria.Dome.Simulation.Physics.Systems;

public static class BottomSlopeCollisionRuleSystem
{
  public static bool ShouldDeferWholeTileCollision(
    byte slope,
    float previousY,
    float absoluteHorizontalVelocity,
    float tileX,
    float tileY,
    float colliderLeft,
    float colliderWidth)
  {
    if (slope == 3)
    {
      return previousY + absoluteHorizontalVelocity >= tileY && colliderLeft >= tileX;
    }

    if (slope == 4)
    {
      return previousY + absoluteHorizontalVelocity >= tileY &&
        colliderLeft + colliderWidth <= tileX + 1.0f;
    }

    return false;
  }
}
