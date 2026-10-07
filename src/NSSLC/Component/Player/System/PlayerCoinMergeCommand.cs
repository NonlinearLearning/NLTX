namespace Terraria.Player;

public readonly record struct PlayerCoinMergeCommand(
  Guid CommandId,
  int SourceSlotIndex);
