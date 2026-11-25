using Terraria.Unshared.World;

namespace Terraria.Share.Liquid;

/// <summary>
/// Binds the concrete dense tile store and world dimensions to the seam the liquid algorithm
/// consumes, so the algorithm depends on a narrow read/write surface instead of world internals.
/// </summary>
/// <remarks>
/// Deliberately thin: no locking, no revision handling, no mutation journaling. Those stay with
/// <see cref="WorldRuntime"/>, which owns them and calls this binding only while holding its lock.
/// </remarks>
internal sealed class LiquidTileStorageBinding : LiquidTileStorage
{
  private readonly WorldRuntimeStateComponent _state;
  private readonly WorldTileStorageComponent _tiles;

  internal LiquidTileStorageBinding(
      WorldRuntimeStateComponent state,
      WorldTileStorageComponent tiles)
  {
    _state = state;
    _tiles = tiles;
  }

  public int Width => _state.Width;

  public int Height => _state.Height;

  public int ToIndex(WorldTileCoordinate coordinate) =>
      checked((coordinate.Y * _state.Width) + coordinate.X);

  public WorldTileCoordinate ToCoordinate(int index)
  {
    int y = index / _state.Width;
    int x = index - (y * _state.Width);
    return new WorldTileCoordinate(x, y);
  }

  public WorldTileState Read(WorldTileCoordinate coordinate) =>
      _tiles.ReadUnchecked(coordinate);

  public void Write(WorldTileCoordinate coordinate, WorldTileState tile) =>
      _tiles.WriteUnchecked(coordinate, tile);
}
