using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Projectile;

/// <summary>Locates the single entity that carries the tick-scoped termination queue.</summary>
internal static class ProjectileTerminationQueue
{
  public static ProjectileTerminationQueueComponent Require(ArchWorld world)
    => SingletonComponent<ProjectileTerminationQueueComponent>.Require(
        world,
        "A ProjectileTerminationQueueComponent entity must exist before terminations are recorded.");
}
