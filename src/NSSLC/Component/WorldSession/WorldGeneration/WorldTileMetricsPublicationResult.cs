namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldTileMetricsPublicationResult(
  bool HasSnapshot,
  WorldTileMetricsSnapshot Snapshot,
  bool ShouldSendMessage57);
