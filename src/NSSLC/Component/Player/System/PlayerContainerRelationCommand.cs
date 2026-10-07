namespace Terraria.Player;

public readonly record struct PlayerContainerRelationCommand(
  Guid CommandId,
  PlayerContainerRelationSlot Slot,
  PlayerContainerRef Container,
  bool IsAvailable,
  bool IsOpen);
