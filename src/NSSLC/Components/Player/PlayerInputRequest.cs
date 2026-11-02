using System.Numerics;
using Terraria.Relationships;

namespace Terraria.Player;

/// <summary>One player state change requested by the communication adapter.</summary>
/// <remarks>
/// The adapter translates packets into intent; the Player domain owns
/// <c>LocationComponent</c> and <c>PlayerInventoryComponent</c> and performs the write, so packet
/// handling never mutates player state directly.
/// </remarks>
public readonly record struct PlayerInputRequest(
    PlayerInputRequestKind Kind,
    EntityRuntimeHandle Player,
    Vector2 Position = default,
    int Slot = 0,
    PlayerInventorySlot InventorySlot = default);
