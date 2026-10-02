namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct SceneMetricsAggregateValue(
  uint ScanRevision,
  uint LastScanTime,
  SceneScanRectangle TileCenter,
  int BestOreType,
  SceneScanRectangle BestOrePosition,
  Guid? PerspectiveEntityId,
  int[] TileCounts,
  int[] LiquidCounts,
  int[] AggregateCounts,
  bool HasBanner,
  bool CanPlayCreditsRoll,
  Guid? ClosestNpcEntityId);
