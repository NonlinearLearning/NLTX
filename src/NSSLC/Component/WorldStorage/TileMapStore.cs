namespace Terraria.WorldStorage;

public sealed class TileMapStore
{
  private TileCellState[,] _tiles = new TileCellState[0, 0];
  private long _mutationRevision;

  public TileMapLayout Layout { get; private set; }
  public int Width => _tiles.GetLength(0);
  public int Height => _tiles.GetLength(1);
  public long MutationRevision => _mutationRevision;
}
