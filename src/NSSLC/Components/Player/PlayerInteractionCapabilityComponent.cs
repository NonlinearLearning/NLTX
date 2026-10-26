using System;

namespace Terraria.Player;

[Flags]
public enum PlayerInteractionCapability : ushort
{
  None = 0,
  PickupItems = 1 << 0,
  UseItems = 1 << 1,
  Craft = 1 << 2,
  ModifyTiles = 1 << 3,
}

/// <summary>Declares authoritative player gameplay capabilities for interaction systems.</summary>
public readonly record struct PlayerInteractionCapabilityComponent(
    PlayerInteractionCapability Capabilities)
{
  public bool Allows(PlayerInteractionCapability capability)
  {
    return capability != PlayerInteractionCapability.None &&
        (Capabilities & capability) == capability;
  }
}
