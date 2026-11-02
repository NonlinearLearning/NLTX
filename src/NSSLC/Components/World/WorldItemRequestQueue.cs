using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.World;

/// <summary>Locates the single entity that carries the world-item request queue.</summary>
internal static class WorldItemRequestQueue
{
  public static WorldItemRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<WorldItemRequestQueueComponent>.Require(
        world,
        "A WorldItemRequestQueueComponent entity must exist before world-item requests are "
        + "recorded.");
}
