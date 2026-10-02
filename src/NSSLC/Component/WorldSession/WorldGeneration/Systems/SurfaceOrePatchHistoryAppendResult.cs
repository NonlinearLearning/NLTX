namespace Terraria.WorldGeneration.Systems;

public readonly record struct SurfaceOrePatchHistoryAppendResult(
  SurfaceOrePatchHistoryAppendStatus Status,
  bool ExternalCommitSucceeded,
  bool Appended);
