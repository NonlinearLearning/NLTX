namespace Terraria.WorldGeneration.Systems;

public readonly record struct LakePlacementHistoryAppendResult(
  LakePlacementHistoryAppendStatus Status,
  bool ExternalCommitSucceeded,
  bool Appended);
