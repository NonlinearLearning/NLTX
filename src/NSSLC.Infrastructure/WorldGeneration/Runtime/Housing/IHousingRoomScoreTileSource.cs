using NSSLC.WorldGeneration.Runtime.Components;

namespace NSSLC.WorldGeneration.Runtime.Housing;

public interface IHousingRoomScoreTileSource
{
  HousingRoomScoreTileSample ReadTile(TilePosition position);

  /// <summary>
  /// Applies the owning world's collision rules to an inclusive tile rectangle.
  /// </summary>
  bool HasSolidTiles(TilePosition minimum, TilePosition maximum);
}
