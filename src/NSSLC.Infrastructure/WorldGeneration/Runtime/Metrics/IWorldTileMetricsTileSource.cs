using NSSLC.WorldGeneration.Runtime.Components;

namespace NSSLC.WorldGeneration.Runtime.Adapters;

public interface IWorldTileMetricsTileSource
{
  WorldTileMetricsTileSample ReadOrCreateTile(int x, int y);
}
