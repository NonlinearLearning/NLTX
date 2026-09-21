using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyPyramidStructureRequest
{
  public const ushort PyramidTileType = 151;
  public const ushort PyramidWallType = 34;
  public const int DefaultPyramidMinDepth = 75;
  public const int DefaultPyramidMaxDepth = 125;

  public int OriginX { get; }

  public int OriginY { get; }

  public int PyramidMinDepth { get; }

  public int PyramidMaxDepth { get; }

  public bool NoTunnel { get; }

  public ushort TileType => PyramidTileType;

  public ushort WallType => PyramidWallType;

  private LegacyPyramidStructureRequest(
    int originX,
    int originY,
    int pyramidMinDepth,
    int pyramidMaxDepth,
    bool noTunnel)
  {
    OriginX = originX;
    OriginY = originY;
    PyramidMinDepth = pyramidMinDepth;
    PyramidMaxDepth = pyramidMaxDepth;
    NoTunnel = noTunnel;
  }

  public static bool TryCreateDefault(
    int originX,
    int originY,
    bool noTunnel,
    out LegacyPyramidStructureRequest request)
  {
    return TryCreate(
      originX,
      originY,
      DefaultPyramidMinDepth,
      DefaultPyramidMaxDepth,
      noTunnel,
      out request);
  }

  public static bool TryCreate(
    int originX,
    int originY,
    int pyramidMinDepth,
    int pyramidMaxDepth,
    bool noTunnel,
    out LegacyPyramidStructureRequest request)
  {
    request = default;
    if (originX < 0 || originY < 0 || pyramidMinDepth <= 0 ||
        pyramidMaxDepth < pyramidMinDepth)
    {
      return false;
    }

    request = new LegacyPyramidStructureRequest(
      originX,
      originY,
      pyramidMinDepth,
      pyramidMaxDepth,
      noTunnel);
    return true;
  }
}
