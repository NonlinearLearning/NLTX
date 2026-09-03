using System;
using Terraria.Dome.Protocol.V1456.Compatibility;

if (LegacyWorldViewLimits.MaximumWidth != 1920 ||
    LegacyWorldViewLimits.MaximumHeight != 1200 ||
    LegacyWorldViewLimits.ClampWidth(0) != 1 ||
    LegacyWorldViewLimits.ClampWidth(2000) != 1920 ||
    LegacyWorldViewLimits.ClampHeight(0) != 1 ||
    LegacyWorldViewLimits.ClampHeight(1300) != 1200)
{
  throw new InvalidOperationException("Legacy world-view limits contract diverged.");
}

Console.WriteLine("PASS: legacy world-view limits=1920x1200");
Console.WriteLine("DEFERRED: client viewport/rendering state remains outside Simulation ECS");
