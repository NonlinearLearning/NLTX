namespace Terraria.NonAuthoritative.ContentDefinitions;

public enum TileObjectDirection
{
  Left,
  Right
}

public sealed class TileObjectGeometryDefinition
{
  public TileObjectGeometryDefinition(
    int tileType,
    int width,
    int height,
    PointValue origin,
    TileObjectDirection direction,
    IReadOnlyList<int> coordinateHeights,
    int coordinateWidth,
    int coordinatePadding)
  {
    if (tileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }

    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    if (coordinateWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(coordinateWidth));
    }

    if (coordinatePadding < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(coordinatePadding));
    }

    ArgumentNullException.ThrowIfNull(coordinateHeights);
    if (coordinateHeights.Count != height)
    {
      throw new ArgumentException("Coordinate heights must match the tile height.", nameof(coordinateHeights));
    }

    TileType = tileType;
    Width = width;
    Height = height;
    Origin = origin;
    Direction = direction;
    CoordinateHeights = coordinateHeights.ToArray();
    CoordinateWidth = coordinateWidth;
    CoordinatePadding = coordinatePadding;
  }

  public int TileType { get; }

  public int DrawYOffset { get; set; }

  public int DrawXOffset { get; set; }

  public bool DrawFlipHorizontal { get; set; }

  public bool DrawFlipVertical { get; set; }

  public int DrawStepDown { get; set; }

  public int Width { get; }

  public int Height { get; }

  public PointValue Origin { get; }

  public TileObjectDirection Direction { get; }

  public bool FlattenAnchors { get; set; }

  public IReadOnlyList<int> CoordinateHeights { get; }

  public int CoordinateWidth { get; }

  public int CoordinatePadding { get; }

  public PointValue CoordinatePaddingFix { get; set; }

  public int CoordinateFullWidth { get; private set; }

  public int CoordinateFullHeight { get; private set; }

  public int DrawStyleOffset { get; set; }

  public IReadOnlyList<RectangleValue> DrawFrameOffsets { get; set; } = Array.Empty<RectangleValue>();

  internal void CalculateDimensions()
  {
    CoordinateFullWidth =
      (CoordinateWidth * Width) + (CoordinatePadding * Math.Max(0, Width - 1));
    CoordinateFullHeight = CoordinateHeights.Sum() + (CoordinatePadding * Math.Max(0, Height - 1));
  }
}

public sealed class TileGeometrySnapshot
{
  internal TileGeometrySnapshot(TileObjectGeometryDefinition definition)
  {
    TileType = definition.TileType;
    Width = definition.Width;
    Height = definition.Height;
    Origin = definition.Origin;
    Direction = definition.Direction;
    CoordinateFullWidth = definition.CoordinateFullWidth;
    CoordinateFullHeight = definition.CoordinateFullHeight;
    DrawStyleOffset = definition.DrawStyleOffset;
  }

  public int TileType { get; }

  public int Width { get; }

  public int Height { get; }

  public PointValue Origin { get; }

  public TileObjectDirection Direction { get; }

  public int CoordinateFullWidth { get; }

  public int CoordinateFullHeight { get; }

  public int DrawStyleOffset { get; }
}

public static class TileGeometryRegistrationSystem
{
  public static void Calculate(TileObjectGeometryDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    definition.CalculateDimensions();
  }
}

public static class TileGeometryQuery
{
  public static TileGeometrySnapshot Snapshot(TileObjectGeometryDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    return new TileGeometrySnapshot(definition);
  }
}

public static class TileDrawProjection
{
  public static RectangleValue Frame(TileGeometrySnapshot geometry, int frameX, int frameY)
  {
    ArgumentNullException.ThrowIfNull(geometry);
    if (frameX < 0 || frameY < 0)
    {
      throw new ArgumentOutOfRangeException();
    }

    return new RectangleValue(
      frameX * geometry.CoordinateFullWidth,
      frameY * geometry.CoordinateFullHeight,
      geometry.CoordinateFullWidth,
      geometry.CoordinateFullHeight);
  }
}
