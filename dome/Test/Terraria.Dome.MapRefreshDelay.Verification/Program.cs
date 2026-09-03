using System;
using Terraria.Dome.Protocol.V1456.Compatibility;

if (LegacyMapRefreshDelayPolicy.DefaultTicks != 2 ||
    LegacyMapRefreshDelayPolicy.Normalize(-1) != 0 ||
    LegacyMapRefreshDelayPolicy.Normalize(2) != 2 ||
    LegacyMapRefreshDelayPolicy.Normalize(121) != 120)
{
  throw new InvalidOperationException("Map refresh delay policy contract diverged.");
}

Console.WriteLine("PASS: legacy map refresh delay default=2 bounded=0..120");
Console.WriteLine("DEFERRED: client map cache lifecycle remains outside Simulation ECS");
