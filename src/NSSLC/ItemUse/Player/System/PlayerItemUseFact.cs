using Terraria.Item;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>One applied use, published so other domains can react without reading player state.</summary>
/// <remarks>
/// A shot is content for the projectile and combat domains: they receive the item, the consumed
/// ammunition and the slot rather than reaching into the player's inventory.
/// </remarks>
public readonly record struct PlayerItemUseFact(
    EntityRuntimeHandle Player,
    short Slot,
    ItemId ItemId,
    ItemId AmmoItemId,
    long Tick,
    bool Continuous,
    PlayerItemUseOutcomeKind Outcome);
