using System;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public static class LegacyMapRefreshDelayPolicy
{
  public const int DefaultTicks = 2;
  public const int MinimumTicks = 0;
  public const int MaximumTicks = 120;

  public static int Normalize(int requestedTicks)
  {
    return Math.Clamp(requestedTicks, MinimumTicks, MaximumTicks);
  }
}
