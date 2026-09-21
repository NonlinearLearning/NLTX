using System;

namespace Terraria.Teleportation;

public static class PylonRefreshPolicy
{
  public const int CooldownTicks = int.MaxValue;

  public static bool ShouldRefresh(int remainingTicks)
  {
    return remainingTicks == 0;
  }

  public static int BeginRefresh()
  {
    return CooldownTicks;
  }

  public static int AdvanceCooldown(int remainingTicks)
  {
    if (remainingTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingTicks));
    }

    return remainingTicks == 0 ? 0 : remainingTicks - 1;
  }
}
