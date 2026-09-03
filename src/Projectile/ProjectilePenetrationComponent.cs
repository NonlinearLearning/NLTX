namespace Terraria.Projectile;

public struct ProjectilePenetrationComponent
{
  public ProjectilePenetrationComponent(
    int remainingHits,
    int maximumHits,
    bool stopsDealingDamageWhenDepleted)
  {
    RemainingHits = remainingHits;
    MaximumHits = maximumHits;
    StopsDealingDamageWhenDepleted = stopsDealingDamageWhenDepleted;
  }

  public int RemainingHits;
  public int MaximumHits;
  public bool StopsDealingDamageWhenDepleted;
}
