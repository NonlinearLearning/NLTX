using System;

namespace Terraria.Dome.Server.Startup;

public static class NpcStreamSpeedPolicy
{
  public const int DefaultTicks = 30;
  public const int MinimumTicks = 1;
  public const int MaximumTicks = 600;

  public static int Normalize(int requestedTicks)
  {
    if (requestedTicks <= 0)
    {
      return DefaultTicks;
    }

    return Math.Clamp(requestedTicks, MinimumTicks, MaximumTicks);
  }
}
