namespace Terraria.WorldGeneration.Systems;

public readonly record struct OasisPlacementHistoryAppendResult(
  OasisPlacementHistoryAppendStatus Status,
  bool ExternalCommitSucceeded,
  bool Appended);
