using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>One cross-domain intention to use or release an item.</summary>
/// <remarks>
/// The Communication layer translates the protocol's item-use input into this request; the Player
/// domain owns the timing gate, the ammo lookup and the channel flag.
/// </remarks>
public readonly record struct PlayerItemUseRequest(
    PlayerItemUseRequestKind Kind,
    EntityRuntimeHandle Player,
    short Slot);
