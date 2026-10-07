using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Housing;

public interface IHousingTileSource
{
  HousingTileSample ReadTile(TilePosition position);
}
