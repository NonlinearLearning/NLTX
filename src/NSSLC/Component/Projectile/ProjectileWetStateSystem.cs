using EntityEcs.Components;

namespace Terraria.Projectile;

/// <summary>Updates persistent wet-state flags and the transition cooldown.</summary>
public static class ProjectileWetStateSystem
{
  private const int ProjectileTypeWithSpecialWetHandling = 155;

  public static ProjectileWetStateAdvanceResult Advance(
    ref ProjectileWetStateComponent state,
    int projectileType,
    bool wetContact,
    bool lavaContact,
    bool honeyContact,
    bool shimmerContact)
  {
    if (state.WetCount < 0)
    {
      throw new InvalidOperationException("Projectile wet-count state cannot be negative.");
    }

    bool wasWet = state.Wet;
    int wetCountBeforeUpdate = state.WetCount;
    if (lavaContact)
    {
      state.LavaWet = true;
    }

    if (honeyContact)
    {
      state.HoneyWet = true;
    }

    if (shimmerContact)
    {
      state.ShimmerWet = true;
    }

    ProjectileWetTransitionKind transition =
      wetContact
        ? wasWet
          ? ProjectileWetTransitionKind.None
          : ProjectileWetTransitionKind.EnteredLiquid
        : wasWet
          ? ProjectileWetTransitionKind.LeftLiquid
          : ProjectileWetTransitionKind.None;
    LiquidKind transitionLiquid = transition == ProjectileWetTransitionKind.None
      ? LiquidKind.Nano
      : GetTransitionLiquid(state);
    bool splashAllowedByTransition = transition ==
      ProjectileWetTransitionKind.EnteredLiquid ||
      transition == ProjectileWetTransitionKind.LeftLiquid && !state.LavaWet;
    bool shouldRequestSplashEffect =
      splashAllowedByTransition &&
      projectileType != ProjectileTypeWithSpecialWetHandling &&
      wetCountBeforeUpdate == 0;

    state.Wet = wetContact;
    if (!wetContact && wasWet &&
        projectileType != ProjectileTypeWithSpecialWetHandling &&
        state.WetCount == 0)
    {
      state.WetCount = ProjectileWetStateComponent.WetTransitionCooldownTicks;
    }

    if (!state.Wet)
    {
      state.LavaWet = false;
      state.HoneyWet = false;
      state.ShimmerWet = false;
    }

    if (state.WetCount > 0)
    {
      state.WetCount--;
    }

    return new ProjectileWetStateAdvanceResult(
      transition,
      transitionLiquid,
      shouldRequestSplashEffect);
  }

  private static LiquidKind GetTransitionLiquid(
    ProjectileWetStateComponent state)
  {
    if (state.LavaWet)
    {
      return LiquidKind.Lava;
    }

    if (state.ShimmerWet)
    {
      return LiquidKind.Shimmer;
    }

    if (state.HoneyWet)
    {
      return LiquidKind.Honey;
    }

    return LiquidKind.Water;
  }
}
