using System;

namespace Terraria.Dome.Protocol.V1456.Compatibility;

public static class LegacyWorldViewLimits
{
  public const int MaximumWidth = 1920;
  public const int MaximumHeight = 1200;

  public static int ClampWidth(int requestedWidth)
  {
    return Math.Clamp(requestedWidth, 1, MaximumWidth);
  }

  public static int ClampHeight(int requestedHeight)
  {
    return Math.Clamp(requestedHeight, 1, MaximumHeight);
  }
}
