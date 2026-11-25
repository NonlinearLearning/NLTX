using ArchWorld = Arch.Core.World;
using Terraria.Share.Entities;

namespace Terraria.Share.Player;

/// <summary>Locates the single entity that carries the item use request queue.</summary>
public static class PlayerItemUseRequestQueue
{
  public static PlayerItemUseRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<PlayerItemUseRequestQueueComponent>.Require(
        world,
        "A PlayerItemUseRequestQueueComponent entity must exist before items can be used.");
}
