using Terraria.Session;
using Terraria.Item;
using Terraria.Relationships;

namespace Terraria.Inventory;

/// <summary>One world item the World domain offers to a player's inventory.</summary>
/// <remarks>
/// The World domain owns world items and the Inventory domain owns stacks. Pickup therefore runs
/// as a request in this direction only: the inventory decides how many units it can accept and
/// reports the accepted quantity back as a
/// <see cref="Terraria.World.WorldItemRequestKind.Consume"/> request.
/// </remarks>
public readonly record struct InventoryPickupRequest(
    EntityRuntimeHandle Player,
    SessionEpoch Actor,
    short Slot,
    ItemStack Stack);
