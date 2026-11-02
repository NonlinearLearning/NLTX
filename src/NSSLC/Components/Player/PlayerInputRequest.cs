using System.Numerics;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>One player state change requested by the communication adapter.</summary>
/// <remarks>
/// The adapter translates packets into intent; the Player domain owns
/// <c>LocationComponent</c> and the player's <c>InventoryComponent</c> and performs the write,
/// so packet handling never mutates player state directly.
/// </remarks>
public readonly record struct PlayerInputRequest(
    PlayerInputRequestKind Kind,
    EntityRuntimeHandle Player,
    Vector2 Position = default,
    int Slot = 0,
    int ItemType = 0,
    int Stack = 0,
    byte Prefix = 0);