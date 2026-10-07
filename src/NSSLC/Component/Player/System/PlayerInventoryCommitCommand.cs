namespace Terraria.Player;

public readonly record struct PlayerInventoryCommitCommand(
  Guid CommandId,
  PlayerInventoryItemSnapshot Item,
  PlayerItemSpaceCandidate Candidate);
