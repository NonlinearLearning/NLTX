using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Inventory;

/// <summary>Locates the single entity that carries the inventory pickup request queue.</summary>
public static class InventoryPickupRequestQueue
{
  public static InventoryPickupRequestQueueComponent Require(ArchWorld world)
    => SingletonComponent<InventoryPickupRequestQueueComponent>.Require(
        world,
        "An InventoryPickupRequestQueueComponent entity must exist before world items can be "
        + "offered to inventories.");
}
