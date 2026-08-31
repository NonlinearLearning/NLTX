using System;
using Terraria.Dome.Simulation.WorldGeneration;

if (LegacyWorldBorderPolicy.OffLimitBorderTiles != 40 ||
    !LegacyWorldBorderPolicy.IsInsideGameplayBounds(40, 40, 200, 120) ||
    !LegacyWorldBorderPolicy.IsInsideGameplayBounds(159, 79, 200, 120) ||
    LegacyWorldBorderPolicy.IsInsideGameplayBounds(39, 40, 200, 120) ||
    LegacyWorldBorderPolicy.IsInsideGameplayBounds(40, 80, 200, 120))
{
  throw new InvalidOperationException("World border policy contract diverged.");
}

Console.WriteLine("PASS: legacy world border policy offset=40");
Console.WriteLine("DEFERRED: full WorldGen placement and tile parity remain outside this slice");
