using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.Projectile.Definitions;

IReadOnlySet<int> hostileTypes = LegacyProjectileHostileRegistry.RegisterDefaults();
if (hostileTypes.Count != 167 ||
    !LegacyProjectileHostileRegistry.IsHostile(12) ||
    LegacyProjectileHostileRegistry.IsHostile(0) ||
    LegacyProjectileHostileRegistry.IsHostile(-1) ||
    LegacyProjectileHostileRegistry.IsHostile(1111))
{
  throw new InvalidOperationException("Projectile hostile registry contract diverged.");
}

int[] orderedTypes = hostileTypes.OrderBy(type => type).ToArray();
if (orderedTypes.First() != 12 || orderedTypes.Last() != 1092)
{
  throw new InvalidOperationException("Projectile hostile source bounds diverged.");
}

Console.WriteLine($"PASS: legacy projectile hostile types={hostileTypes.Count} domain=1111");
Console.WriteLine("DEFERRED: conditional and unmodeled Projectile.SetDefaults behavior remains outside this slice");
