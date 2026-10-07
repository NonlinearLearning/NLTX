namespace Terraria.Player;

public readonly record struct PlayerInventoryPickupCommand(
  Guid CommandId,
  PlayerInventoryItemSnapshot Item,
  PlayerItemSpaceCandidate Candidate,
  PlayerInventoryTransferSettings Settings);
