using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Adapters;

public interface IWorldTileMetricsTileSource
{
  WorldTileMetricsTileSample ReadOrCreateTile(int x, int y);
}
