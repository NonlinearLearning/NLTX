namespace NLTX.PlayerInputGameplay.Pickup;

public readonly record struct PlayerItemPickupLogEntry(
  int TargetArrayId,
  int TargetSlot,
  int TargetItemSlotContext,
  int Stack);
