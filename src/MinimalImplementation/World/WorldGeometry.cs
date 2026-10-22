namespace MinimalImplementation.Server.World;

public readonly record struct WorldGeometry(int Width, int Height)
{
  public const int SectionWidth = 200;
  public const int SectionHeight = 150;
  public const long MaximumTileCount = 25_000_000;

  public int TileCount => checked(Width * Height);

  public int SectionCountX => (Width + SectionWidth - 1) / SectionWidth;

  public int SectionCountY => (Height + SectionHeight - 1) / SectionHeight;

  public bool Contains(TileCoordinate coordinate)
  {
    return coordinate.X >= 0 && coordinate.Y >= 0 &&
      coordinate.X < Width && coordinate.Y < Height;
  }

  public bool ContainsSection(WorldSectionCoordinate coordinate)
  {
    return coordinate.X >= 0 && coordinate.Y >= 0 &&
      coordinate.X < SectionCountX && coordinate.Y < SectionCountY;
  }

  public WorldSectionBounds GetSectionBounds(WorldSectionCoordinate coordinate)
  {
    if (!ContainsSection(coordinate))
    {
      throw new ArgumentOutOfRangeException(nameof(coordinate));
    }

    int originX = checked(coordinate.X * SectionWidth);
    int originY = checked(coordinate.Y * SectionHeight);
    return new WorldSectionBounds(
      originX,
      originY,
      Math.Min(SectionWidth, Width - originX),
      Math.Min(SectionHeight, Height - originY));
  }

  /// <summary>
  /// Returns the stable row-major section set that intersects a tile rectangle, clipping the
  /// rectangle at world edges. The rectangle uses an exclusive right and bottom edge.
  /// </summary>
  public IReadOnlyList<WorldSectionCoordinate> GetIntersectingSections(
    WorldSectionBounds tileRectangle)
  {
    if (Width <= 0 || Height <= 0 || tileRectangle.Width <= 0 || tileRectangle.Height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileRectangle));
    }

    long rectangleRight = (long)tileRectangle.OriginX + tileRectangle.Width;
    long rectangleBottom = (long)tileRectangle.OriginY + tileRectangle.Height;
    int left = Math.Max(0, tileRectangle.OriginX);
    int top = Math.Max(0, tileRectangle.OriginY);
    int right = (int)Math.Min(Width, rectangleRight);
    int bottom = (int)Math.Min(Height, rectangleBottom);
    if (left >= right || top >= bottom)
    {
      return Array.Empty<WorldSectionCoordinate>();
    }

    int firstSectionX = left / SectionWidth;
    int lastSectionX = (right - 1) / SectionWidth;
    int firstSectionY = top / SectionHeight;
    int lastSectionY = (bottom - 1) / SectionHeight;
    int sectionCount = checked(
      (lastSectionX - firstSectionX + 1) * (lastSectionY - firstSectionY + 1));
    var sections = new List<WorldSectionCoordinate>(sectionCount);
    for (int sectionY = firstSectionY; sectionY <= lastSectionY; sectionY++)
    {
      for (int sectionX = firstSectionX; sectionX <= lastSectionX; sectionX++)
      {
        sections.Add(new WorldSectionCoordinate(sectionX, sectionY));
      }
    }

    return Array.AsReadOnly(sections.ToArray());
  }

  /// <summary>Maps a pixel position to the section containing its tile.</summary>
  public WorldSectionCoordinate GetSectionForPixelPosition(float pixelX, float pixelY)
  {
    if (!float.IsFinite(pixelX) || !float.IsFinite(pixelY))
    {
      throw new ArgumentOutOfRangeException(nameof(pixelX));
    }

    int tileX = Math.Clamp((int)MathF.Floor(pixelX / 16f), 0, Width - 1);
    int tileY = Math.Clamp((int)MathF.Floor(pixelY / 16f), 0, Height - 1);
    return new WorldSectionCoordinate(tileX / SectionWidth, tileY / SectionHeight);
  }

  /// <summary>Returns a stable y-then-x section neighborhood around a pixel position.</summary>
  public IReadOnlyList<WorldSectionCoordinate> GetSectionsAroundPixelPosition(
    float pixelX,
    float pixelY,
    int radius)
  {
    if (radius < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(radius));
    }

    WorldSectionCoordinate center = GetSectionForPixelPosition(pixelX, pixelY);
    int minX = Math.Max(0, center.X - radius);
    int maxX = Math.Min(SectionCountX - 1, center.X + radius);
    int minY = Math.Max(0, center.Y - radius);
    int maxY = Math.Min(SectionCountY - 1, center.Y + radius);
    var result = new List<WorldSectionCoordinate>((maxX - minX + 1) * (maxY - minY + 1));
    for (int y = minY; y <= maxY; y++)
    {
      for (int x = minX; x <= maxX; x++)
      {
        result.Add(new WorldSectionCoordinate(x, y));
      }
    }

    return Array.AsReadOnly(result.ToArray());
  }
}

public readonly record struct WorldSectionCoordinate(int X, int Y)
{
  public override string ToString() => $"({X},{Y})";
}

public readonly record struct WorldSectionBounds(int OriginX, int OriginY, int Width, int Height)
{
  public bool Contains(TileCoordinate coordinate)
  {
    return (long)coordinate.X >= OriginX && (long)coordinate.Y >= OriginY &&
      (long)coordinate.X < (long)OriginX + Width &&
      (long)coordinate.Y < (long)OriginY + Height;
  }
}

public enum TileWireOrder
{
  NetworkYThenX,
  FileXThenY
}
