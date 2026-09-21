namespace Terraria.Combat.Targeting;

public static class TargetValidityQuery
{
  public static bool IsInvalid(CombatTargetSnapshot target)
  {
    return target.IsInvalid;
  }

  public static bool IsWithinRange(
    CombatTargetSnapshot target,
    System.Numerics.Vector2 origin,
    float maximumDistance)
  {
    if (maximumDistance < 0f || target.IsInvalid)
    {
      return false;
    }

    return System.Numerics.Vector2.DistanceSquared(origin, target.Center) <=
      maximumDistance * maximumDistance;
  }
}
