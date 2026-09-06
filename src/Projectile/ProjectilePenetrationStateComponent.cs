namespace Terraria.Projectile;

public struct ProjectilePenetrationStateComponent
{
  public ProjectilePenetrationStateComponent(
    int remainingHits = 1,
    int maximumHits = 1,
    int hitCount = 0,
    bool stopsDealingDamageWhenDepleted = false)
  {
    RemainingHits = remainingHits;
    MaximumHits = maximumHits;
    HitCount = hitCount;
    StopsDealingDamageWhenDepleted = stopsDealingDamageWhenDepleted;
  }

  public int RemainingHits;
  public int MaximumHits;
  public int HitCount;
  public bool StopsDealingDamageWhenDepleted;
}
