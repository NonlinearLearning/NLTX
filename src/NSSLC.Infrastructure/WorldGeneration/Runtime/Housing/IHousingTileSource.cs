using NSSLC.WorldGeneration.Runtime.Components;

namespace NSSLC.WorldGeneration.Runtime.Housing;

public interface IHousingTileSource
{
  HousingTileSample ReadTile(TilePosition position);
}
