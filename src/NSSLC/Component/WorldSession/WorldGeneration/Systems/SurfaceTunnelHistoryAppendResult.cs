namespace Terraria.WorldGeneration.Systems;

public readonly record struct SurfaceTunnelHistoryAppendResult(
  SurfaceTunnelHistoryAppendStatus Status,
  bool ScanAccepted,
  bool Appended);
