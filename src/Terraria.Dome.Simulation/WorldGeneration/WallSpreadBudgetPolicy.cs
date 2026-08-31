using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WallSpreadBudgetPolicy
{
  public const int Version4MaximumCount = 5000;

  public static WallSpreadBudgetDecision Create(int maximumCount = Version4MaximumCount)
  {
    if (maximumCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumCount));
    }

    return new WallSpreadBudgetDecision(0, maximumCount, true, false);
  }

  public static WallSpreadBudgetDecision Apply(
    WallSpreadBudgetDecision decision)
  {
    if (decision.MaximumCount <= 0 || decision.AppliedCount < 0 ||
        decision.AppliedCount > decision.MaximumCount)
    {
      throw new ArgumentOutOfRangeException(nameof(decision));
    }

    if (!decision.CanSpread || decision.AppliedCount >= decision.MaximumCount)
    {
      return decision with { CanSpread = false, IsExhausted = true };
    }

    int appliedCount = checked(decision.AppliedCount + 1);
    return new WallSpreadBudgetDecision(
      appliedCount,
      decision.MaximumCount,
      appliedCount < decision.MaximumCount,
      appliedCount >= decision.MaximumCount);
  }
}
