namespace Terraria.Player;

public static class PlayerChannelCancellationAdapter
{
  public static bool Matches(
    in PlayerChannelCancellationExpectation expectation,
    in PlayerChannelCancellationProjectileSnapshot projectile)
  {
    if (projectile.AiStyle == 99 && projectile.Ai0 == -3f)
    {
      return true;
    }

    return expectation.ProjectileTypeExpected == projectile.ProjectileType &&
      expectation.ProjectileIndexExpected == projectile.ProjectileIndex;
  }

  public static PlayerChannelCancellationExpectation Track(
    in PlayerChannelCancellationExpectation expectation,
    in PlayerChannelCancellationProjectileSnapshot projectile)
  {
    if (expectation.ProjectileTypeExpected != projectile.ProjectileType)
    {
      return expectation;
    }

    return expectation with { ProjectileIndexExpected = projectile.ProjectileIndex };
  }
}
