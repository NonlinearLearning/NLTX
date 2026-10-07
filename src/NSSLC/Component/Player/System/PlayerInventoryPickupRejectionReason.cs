namespace Terraria.Player;

public enum PlayerInventoryPickupRejectionReason : byte
{
  None,
  EmptyCommand,
  InventoryCommitRejected,
  EffectPortRejected,
}
