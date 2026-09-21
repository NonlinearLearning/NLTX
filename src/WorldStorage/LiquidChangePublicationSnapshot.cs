using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

public sealed class LiquidChangePublicationSnapshot
{
  private readonly IReadOnlyList<TileCoordinate> _coordinates;
  private readonly IReadOnlyList<int> _encodedCoordinates;

  public LiquidChangePublicationSnapshot(
    long revision,
    IReadOnlyCollection<TileCoordinate> coordinates)
  {
    ArgumentNullException.ThrowIfNull(coordinates);
    if (revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(revision));
    }

    List<TileCoordinate> coordinateCopy = new(coordinates);
    List<int> encodedCopy = new(coordinateCopy.Count);
    for (int index = 0; index < coordinateCopy.Count; index++)
    {
      encodedCopy.Add(EncodeCoordinate(coordinateCopy[index]));
    }

    Revision = revision;
    _coordinates = coordinateCopy.AsReadOnly();
    _encodedCoordinates = encodedCopy.AsReadOnly();
  }

  public long Revision { get; }

  public int Count => _coordinates.Count;

  public bool IsEmpty => _coordinates.Count == 0;

  public IReadOnlyList<TileCoordinate> Coordinates => _coordinates;

  public IReadOnlyList<int> EncodedCoordinates => _encodedCoordinates;

  public static int EncodeCoordinate(TileCoordinate coordinate)
  {
    return (coordinate.X & 0xFFFF) << 16 | coordinate.Y & 0xFFFF;
  }

  public static LiquidChangePublicationSnapshot Empty(long revision)
  {
    return new LiquidChangePublicationSnapshot(
      revision,
      Array.Empty<TileCoordinate>());
  }
}
