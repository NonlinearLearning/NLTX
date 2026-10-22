using System.Collections.ObjectModel;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Captures a device refresh at its position in the wire-color traversal.</summary>
public sealed class WiringTileSquareEffect
{
  internal WiringTileSquareEffect(
    WorldSectionBounds bounds,
    IReadOnlyDictionary<TileCoordinate, WorldTile> tiles,
    short? animationType = null,
    ushort animationTileType = 0,
    int animationSendCount = 0)
  {
    Bounds = bounds;
    Tiles = new ReadOnlyDictionary<TileCoordinate, WorldTile>(
      tiles.ToDictionary(pair => pair.Key, pair => pair.Value));
    AnimationType = animationType;
    AnimationTileType = animationTileType;
    AnimationSendCount = animationSendCount;
  }

  public WorldSectionBounds Bounds { get; }

  public IReadOnlyDictionary<TileCoordinate, WorldTile> Tiles { get; }

  public short? AnimationType { get; }

  public ushort AnimationTileType { get; }

  public int AnimationSendCount { get; }
}
