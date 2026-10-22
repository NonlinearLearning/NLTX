using NSSLC.WorldGeneration.Runtime.Components;

namespace NSSLC.WorldGeneration.Runtime;

/// <summary>Working state owned solely by one serialized generation lifecycle.</summary>
public sealed class GenerationRuntimeState {
  public GenerationTileMetricsState TileMetrics { get; } = new();
  public GenerationHousingRegistryState TownHousing { get; } = new();
}
