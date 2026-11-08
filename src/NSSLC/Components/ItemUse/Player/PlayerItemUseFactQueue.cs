using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Player;

/// <summary>Locates the single entity that carries the applied item use facts.</summary>
public static class PlayerItemUseFactQueue
{
  public static PlayerItemUseFactQueueComponent Require(ArchWorld world)
    => SingletonComponent<PlayerItemUseFactQueueComponent>.Require(
        world,
        "A PlayerItemUseFactQueueComponent entity must exist before item use facts are read.");
}
