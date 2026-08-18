using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldGridSnapshot
{
  private readonly long[,] _sectionVersions;
  private readonly WorldTile[,] _tiles;

  public WorldGridSnapshot(
    WorldMetadata metadata,
    WorldTile[,] tiles,
    long[,] sectionVersions)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentNullException.ThrowIfNull(sectionVersions);
    ArgumentNullException.ThrowIfNull(tiles);
    if (tiles.GetLength(0) != metadata.Width || tiles.GetLength(1) != metadata.Height)
    {
      throw new ArgumentException(
        "Tile dimensions do not match the world metadata.",
        nameof(tiles));
    }

    if (metadata.Width % WorldGrid.SectionWidth != 0 ||
        metadata.Height % WorldGrid.SectionHeight != 0)
    {
      throw new ArgumentException(
        "World metadata dimensions must be whole Terraria section units.",
        nameof(metadata));
    }

    if (sectionVersions.GetLength(0) != metadata.Width / WorldGrid.SectionWidth ||
        sectionVersions.GetLength(1) != metadata.Height / WorldGrid.SectionHeight)
    {
      throw new ArgumentException(
        "Section-version dimensions do not match the world metadata.",
        nameof(sectionVersions));
    }

    Metadata = metadata;
    _tiles = (WorldTile[,])tiles.Clone();
    _sectionVersions = (long[,])sectionVersions.Clone();
  }

  public WorldMetadata Metadata { get; }

  public long GetSectionVersion(WorldSectionCoordinates coordinates)
  {
    ValidateSectionCoordinates(coordinates);
    return _sectionVersions[coordinates.X, coordinates.Y];
  }

  public WorldTile GetTile(int x, int y)
  {
    ValidateTileCoordinates(x, y);
    return _tiles[x, y];
  }

  internal long[,] CopySectionVersions()
  {
    return (long[,])_sectionVersions.Clone();
  }

  internal WorldTile[,] CopyTiles()
  {
    return (WorldTile[,])_tiles.Clone();
  }

  private void ValidateSectionCoordinates(WorldSectionCoordinates coordinates)
  {
    if (coordinates.X < 0 || coordinates.X >= _sectionVersions.GetLength(0) ||
        coordinates.Y < 0 || coordinates.Y >= _sectionVersions.GetLength(1))
    {
      throw new ArgumentOutOfRangeException(nameof(coordinates));
    }
  }

  private void ValidateTileCoordinates(int x, int y)
  {
    if (x < 0 || x >= Metadata.Width || y < 0 || y >= Metadata.Height)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }
  }
}
