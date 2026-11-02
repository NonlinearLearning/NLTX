using Terraria.World;

namespace Terraria.Liquid;

/// <summary>
/// The tile access surface the liquid algorithm needs: read and write single tiles by
/// coordinate, plus the world dimensions.
/// </summary>
/// <remarks>
/// This is the seam that keeps <see cref="LiquidSimulation"/> independent of the concrete
/// dense tile store. Implementations must be cheap and side-effect free beyond the tile writes
/// they are asked to perform; the algorithm is synchronous and single-threaded by contract.
/// </remarks>
internal interface LiquidTileStorage
{
  int Width { get; }

  int Height { get; }

  int ToIndex(WorldTileCoordinate coordinate);

  WorldTileCoordinate ToCoordinate(int index);

  WorldTileState Read(WorldTileCoordinate coordinate);

  void Write(WorldTileCoordinate coordinate, WorldTileState tile);
}
