namespace Terraria.Unshared.Player;

/// <summary>Kind of player state change a foreign domain may request.</summary>
public enum PlayerInputRequestKind : byte
{
  /// <summary>Overwrite the authoritative coordinates from client input.</summary>
  SetPosition,

  /// <summary>Overwrite one inventory slot from client input.</summary>
  SetInventorySlot,

  /// <summary>Point the inventory's held slot at the slot the client selected.</summary>
  SelectHeldSlot
}
