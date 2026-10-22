namespace MinimalImplementation.Server.World;

/// <summary>
/// Carries Version4's weighted world-alignment totals and packet 57 percentages.
/// </summary>
public readonly record struct WorldAlignmentMetrics(
  long GoodTileWeight,
  long EvilTileWeight,
  long BloodTileWeight,
  long SolidTileWeight,
  byte GoodPercent,
  byte EvilPercent,
  byte BloodPercent);

/// <summary>
/// Calculates the world-alignment values serialized by Version4 packet 57.
/// </summary>
/// <remarks>
/// Behavior source: Version4 Terraria.WorldGen.CountTiles and AddUpAlignmentCounts,
/// plus TileID.Sets.HallowCountCollection, CorruptCountCollection, and CrimsonCountCollection.
/// Source SHA256: WorldGen.cs A06A8463E39EA065441FF1EF702A787D3450ADAE5CD3065CF30BF76784D3EA1D;
/// TileID.cs 60181CCC3AF26DDAEF55D14774163A124F3D7F494DD999E1BAB6D125AE6BB1A9.
/// </remarks>
public static class WorldAlignmentMetricsCalculator
{
  private static readonly bool[] HallowTiles = CreateLookup(
    [109, 492, 117, 116, 164, 402, 403, 115, 110, 113]);

  private static readonly bool[] CorruptTiles = CreateLookup(
    [23, 661, 25, 112, 163, 398, 400, 636, 24, 32]);

  private static readonly bool[] CrimsonTiles = CreateLookup(
    [199, 662, 203, 234, 200, 399, 401, 205, 201, 352]);

  private static readonly bool[] BaseSolidTiles = CreateLookup([2, 477, 1, 60, 53, 161]);

  public static WorldAlignmentMetrics Calculate(
    WorldHeaderSnapshot header,
    IReadOnlyList<WorldTile> tiles)
  {
    ArgumentNullException.ThrowIfNull(header);
    ArgumentNullException.ThrowIfNull(tiles);

    WorldGeometry geometry = header.Geometry;
    long expectedTileCount = (long)geometry.Width * geometry.Height;
    if (geometry.Width <= 0 || geometry.Height <= 0 || expectedTileCount != tiles.Count)
    {
      throw new ArgumentException("Tile count does not match the world geometry.", nameof(tiles));
    }

    long goodTileWeight = 0;
    long evilTileWeight = 0;
    long bloodTileWeight = 0;
    long solidTileWeight = 0;
    int lastScannedY = geometry.Height - 40;
    int surfaceBoundary = GetSurfaceBoundary(header.Facts.WorldSurface, lastScannedY);

    int firstScannedY = Math.Min(40, surfaceBoundary);
    for (int y = firstScannedY; y < lastScannedY; y++)
    {
      int weight = y < surfaceBoundary ? 5 : 1;
      int rowOffset = y * geometry.Width;
      for (int x = 0; x < geometry.Width; x++)
      {
        WorldTile tile = tiles[rowOffset + x];
        if (!tile.Active)
        {
          continue;
        }

        ushort type = tile.Type;
        if (Contains(HallowTiles, type))
        {
          goodTileWeight += weight;
        }

        if (Contains(CorruptTiles, type))
        {
          evilTileWeight += weight;
        }

        if (Contains(CrimsonTiles, type))
        {
          bloodTileWeight += weight;
        }

        if (header.Facts.RemixWorld && type == 474)
        {
          evilTileWeight += weight;
        }

        if (header.Facts.RemixWorld && type == 195)
        {
          bloodTileWeight += weight;
        }

        if (Contains(BaseSolidTiles, type) ||
            Contains(HallowTiles, type) ||
            Contains(CorruptTiles, type) ||
            Contains(CrimsonTiles, type) ||
            (header.Facts.RemixWorld && (type == 474 || type == 195)))
        {
          solidTileWeight += weight;
        }
      }
    }

    return new WorldAlignmentMetrics(
      goodTileWeight,
      evilTileWeight,
      bloodTileWeight,
      solidTileWeight,
      ToPercentage(goodTileWeight, solidTileWeight),
      ToPercentage(evilTileWeight, solidTileWeight),
      ToPercentage(bloodTileWeight, solidTileWeight));
  }

  private static int GetSurfaceBoundary(double worldSurface, int lastScannedY)
  {
    if (!double.IsFinite(worldSurface))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurface));
    }

    double boundary = worldSurface + 1.0;
    if (boundary < 0 || boundary > lastScannedY)
    {
      throw new ArgumentOutOfRangeException(
        nameof(worldSurface),
        "The Version4 surface boundary must be within the scanned world rows.");
    }

    return (int)boundary;
  }

  private static bool[] CreateLookup(IReadOnlyList<ushort> tileTypes)
  {
    var lookup = new bool[WorldFacts.TileCount];
    for (int index = 0; index < tileTypes.Count; index++)
    {
      lookup[tileTypes[index]] = true;
    }

    return lookup;
  }

  private static bool Contains(bool[] lookup, ushort type)
  {
    return type < lookup.Length && lookup[type];
  }

  private static byte ToPercentage(long alignmentTileWeight, long solidTileWeight)
  {
    if (solidTileWeight == 0)
    {
      return 0;
    }

    byte percentage = (byte)Math.Round(
      ((double)alignmentTileWeight / solidTileWeight) * 100.0);
    if (percentage == 0 && alignmentTileWeight > 0)
    {
      return 1;
    }

    return percentage;
  }
}
