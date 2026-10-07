using System;

namespace Terraria.Projectile;

public static class ProjectileLifetimeSystem
{
  public static bool Advance(ref ProjectileLifetimeStateComponent state)
  {
    if (!state.Active)
    {
      return false;
    }

    if (state.TimeLeft <= 1)
    {
      state.TimeLeft = 0;
      state.Active = false;
      state.EndReason = ProjectileEndReason.LifetimeExpired;
      return true;
    }

    state.TimeLeft--;
    return false;
  }

  public static bool CommitTermination(
    ref ProjectileLifetimeStateComponent state,
    ProjectileEndReason reason)
  {
    if (reason is ProjectileEndReason.None or ProjectileEndReason.WorldBoundary)
    {
      throw new ArgumentOutOfRangeException(nameof(reason));
    }

    if (!state.Active && state.TimeLeft == 0)
    {
      return false;
    }

    state.Active = false;
    state.TimeLeft = 0;
    state.EndReason = reason;
    return true;
  }

  public static bool CommitWorldBoundaryDeactivation(
    ref ProjectileLifetimeStateComponent state)
  {
    if (!state.Active)
    {
      return false;
    }

    state.Active = false;
    state.EndReason = ProjectileEndReason.WorldBoundary;
    return true;
  }
}
