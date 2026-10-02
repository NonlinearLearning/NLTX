using System;

namespace Terraria.Projectile;

public static class ProjectilePenetrationSystem
{
  public static void Initialize(
    ref ProjectilePenetrationStateComponent state,
    int remainingHits,
    int maximumHits,
    bool stopsDealingDamageWhenDepleted)
  {
    state = new ProjectilePenetrationStateComponent(
      remainingHits,
      maximumHits,
      hitCount: 0,
      stopsDealingDamageWhenDepleted);
  }

  public static bool CanDealDamage(
    in ProjectilePenetrationStateComponent state)
  {
    return state.HasRemainingHits;
  }

  public static bool CommitAcceptedHit(
    ref ProjectilePenetrationStateComponent state)
  {
    if (!CanDealDamage(state))
    {
      return false;
    }

    state.HitCount = checked(state.HitCount + 1);
    if (!state.IsUnlimited)
    {
      state.RemainingHits--;
    }

    return true;
  }

  public static void SetRemainingHits(
    ref ProjectilePenetrationStateComponent state,
    int remainingHits)
  {
    if (remainingHits < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingHits));
    }

    state.RemainingHits = remainingHits;
  }
}
