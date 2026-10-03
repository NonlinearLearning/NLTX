using NSSLC.WorldGeneration.Geometry;

namespace NSSLC.WorldGeneration.DataStructures;

public abstract class AEntitySource_Tile : IEntitySource
{
	public readonly Point TileCoords;

	public AEntitySource_Tile(int tileCoordsX, int tileCoordsY)
	{
		TileCoords = new Point(tileCoordsX, tileCoordsY);
	}
}
