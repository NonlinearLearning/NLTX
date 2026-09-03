using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WeaponsRackValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int RackWidth = 3;
  private const int RackHeight = 3;
  private const int WeaponRackTileType = 334;
  private const int EncodedFrameThreshold = 5000;

  public static WeaponsRackValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    int sourceFrameX = NormalizeFrame(source.FrameX);
    int originX = x - sourceFrameX / TileFrameWidth;
    int originY = y - source.FrameY / TileFrameWidth;
    bool valid = true;
    bool hasWallBackings = true;
    for (int offsetX = 0; offsetX < RackWidth; offsetX++)
    {
      for (int offsetY = 0; offsetY < RackHeight; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        WorldTile tile = snapshot.Metadata.IsInside(tileX, tileY)
          ? snapshot.GetTile(tileX, tileY)
          : default;
        bool tileValid = tile.IsActive && tile.Type == WeaponRackTileType &&
          tile.WallType > 0 &&
          NormalizeFrame(tile.FrameX) == offsetX * TileFrameWidth &&
          tile.FrameY == offsetY * TileFrameWidth;
        valid &= tileValid;
        hasWallBackings &= tile.WallType > 0;
      }
    }

    valid &= hasWallBackings;
    return new WeaponsRackValidationResult(valid, !valid, originX, originY, hasWallBackings);
  }

  private static int NormalizeFrame(short frameX)
  {
    int normalized = frameX;
    if (normalized >= EncodedFrameThreshold)
    {
      int encodedBand = normalized / EncodedFrameThreshold;
      normalized = (encodedBand - 1) * TileFrameWidth;
    }

    return normalized % (RackWidth * TileFrameWidth);
  }
}
