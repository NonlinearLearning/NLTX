using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.World;

/// <summary>Locates the single entity that carries the tile entity request queue.</summary>
public static class WorldTileEntityRequestQueue
{
  public static WorldTileEntityRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<WorldTileEntityRequestQueueComponent>.Require(
        world,
        "A WorldTileEntityRequestQueueComponent entity must exist before chests can be opened.");
}
