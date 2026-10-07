namespace Terraria.Projectile;

public struct ProjectileWetStateComponent
{
  public const int WetTransitionCooldownTicks = 10;

  public bool Wet;
  public bool LavaWet;
  public bool HoneyWet;
  public bool ShimmerWet;
  public int WetCount;
}
