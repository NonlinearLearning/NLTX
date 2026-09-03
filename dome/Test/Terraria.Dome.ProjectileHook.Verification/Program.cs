using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.Projectile.Definitions;

if (LegacyProjectileHookRegistry.ProjectileTypeCount != 1111)
{
  throw new InvalidOperationException("Projectile hook domain changed.");
}

if (LegacyProjectileHookRegistry.IsHook(-1) ||
    LegacyProjectileHookRegistry.IsHook(LegacyProjectileHookRegistry.ProjectileTypeCount))
{
  throw new InvalidOperationException("Projectile hook lookup must fail closed outside the domain.");
}

IReadOnlySet<int> hooks = LegacyProjectileHookRegistry.RegisterDefaults();
int[] expectedTypes =
[
  13, 32, 72, 165, 229, 256, 315, 322, 331, 332,
  372, 396, 403, 446, 489, 645, 652, 753, 865, 935
];
if (hooks.Count != expectedTypes.Length || expectedTypes.Any(type => !hooks.Contains(type)))
{
  throw new InvalidOperationException("Projectile hook coverage diverged from the legacy source.");
}

foreach (int projectileType in expectedTypes)
{
  if (!LegacyProjectileHookRegistry.IsHook(projectileType))
  {
    throw new InvalidOperationException(
      $"Projectile hook classification diverged for type {projectileType}.");
  }
}

Console.WriteLine($"PASS: modeled projectile hook types={hooks.Count} domain=1111");
Console.WriteLine("DEFERRED: unmodeled legacy Projectile.SetDefaults classifications remain outside this slice");
