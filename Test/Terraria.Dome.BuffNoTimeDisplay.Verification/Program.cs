using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.StatusEffects.Definitions;

IReadOnlySet<int> types = LegacyBuffNoTimeDisplayRegistry.RegisterDefaults();
if (types.Count != 118 ||
    !LegacyBuffNoTimeDisplayRegistry.IsBuffNoTimeDisplay(19) ||
    !LegacyBuffNoTimeDisplayRegistry.IsBuffNoTimeDisplay(304) ||
    LegacyBuffNoTimeDisplayRegistry.IsBuffNoTimeDisplay(0) ||
    LegacyBuffNoTimeDisplayRegistry.IsBuffNoTimeDisplay(-1) ||
    LegacyBuffNoTimeDisplayRegistry.IsBuffNoTimeDisplay(
      LegacyBuffNoTimeDisplayRegistry.BuffTypeCount))
{
  throw new InvalidOperationException("Buff no-time-display registry contract diverged.");
}

Console.WriteLine($"PASS: legacy buff-no-time-display types={types.Count} domain=389");
Console.WriteLine("DEFERRED: client presentation and complete buff lifecycle remain outside this slice");
