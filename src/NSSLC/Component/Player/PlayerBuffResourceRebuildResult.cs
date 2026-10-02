namespace Terraria.Player;

public readonly record struct PlayerBuffResourceRebuildResult(
  bool Applied,
  PlayerBuffResourceSnapshot Snapshot,
  PlayerBuffResourceRebuildRejectionReason RejectionReason);
