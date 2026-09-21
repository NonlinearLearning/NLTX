namespace Terraria.SpatialMotionPhysics;

public sealed class DarknessHazardDefinition
{
  public DarknessHazardDefinition(int hitTimerMaxBeforeHit)
  {
    if (hitTimerMaxBeforeHit <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(hitTimerMaxBeforeHit));
    }

    HitTimerMaxBeforeHit = hitTimerMaxBeforeHit;
  }

  public int HitTimerMaxBeforeHit { get; }
}
