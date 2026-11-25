using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Projectiles;

/// <summary>Locates the single entity that carries the tick-scoped spawn queue.</summary>
internal static class ProjectileSpawnQueue
{
  public static ProjectileSpawnQueueComponent Require(ArchWorld world)
    => SingletonComponent<ProjectileSpawnQueueComponent>.Require(
        world,
        "A ProjectileSpawnQueueComponent entity must exist before spawns are recorded.");
}
