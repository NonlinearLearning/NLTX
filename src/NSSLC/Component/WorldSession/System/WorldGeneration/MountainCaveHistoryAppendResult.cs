namespace Terraria.WorldGeneration.Systems;

public readonly record struct MountainCaveHistoryAppendResult(
  MountainCaveHistoryAppendStatus Status,
  bool ExternalCommitSucceeded,
  bool Appended);
