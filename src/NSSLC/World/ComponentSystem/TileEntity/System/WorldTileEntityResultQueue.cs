using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Unshared.World;

/// <summary>Locates the single entity that carries the tile entity result queue.</summary>
public static class WorldTileEntityResultQueue
{
  public static WorldTileEntityResultQueueComponent Require(ArchWorld world)
    => SingletonComponent<WorldTileEntityResultQueueComponent>.Require(
        world,
        "A WorldTileEntityResultQueueComponent entity must exist before chest results are read.");
}
