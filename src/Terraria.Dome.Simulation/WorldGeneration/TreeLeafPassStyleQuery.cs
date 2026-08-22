using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeLeafPassStyleQuery
{
  private const int HollowTreeFoliageStyle = 20;
  private const ushort AshTreeTileType = 633;
  private const ushort WillowTreeTileType = 616;
  private const ushort SakuraTreeTileType = 596;

  public static TreeLeafPassStyleResult Evaluate(
    int x,
    WorldTile topTile,
    WorldTile groundTile,
    int treeHeight,
    int hollowTreeFoliageStyle)
  {
    int treeFrame = GetInitialTreeFrame(topTile);
    int passStyle = GetProfilePassStyle(topTile.Type);
    if (passStyle >= 0)
    {
      return new TreeLeafPassStyleResult(treeFrame, passStyle, treeHeight);
    }

    passStyle = groundTile.Type switch
    {
      2 or 477 => 910,
      60 => 914,
      70 => 912,
      23 or 112 => 915,
      199 or 234 => 916,
      53 => 911,
      116 => 919,
      147 => 913,
      AshTreeTileType => 1278,
      109 or 492 => GetHollowTreePassStyle(x, treeFrame, hollowTreeFoliageStyle, out treeFrame),
      _ => -1
    };

    int adjustedHeight = groundTile.Type is 109 or 492 ? treeHeight + 5 : treeHeight;
    return new TreeLeafPassStyleResult(treeFrame, passStyle, adjustedHeight);
  }

  private static int GetInitialTreeFrame(WorldTile topTile)
  {
    if (topTile.FrameX is 22 or 44 or 66)
    {
      return topTile.FrameY switch
      {
        220 => 1,
        242 => 2,
        _ => 0
      };
    }

    return 0;
  }

  private static int GetProfilePassStyle(ushort tileType)
  {
    return tileType switch
    {
      SakuraTreeTileType => 1248,
      WillowTreeTileType => 1257,
      AshTreeTileType => 1278,
      >= 583 and <= 589 => 1249 + tileType - 583,
      _ => -1
    };
  }

  private static int GetHollowTreePassStyle(
    int x,
    int initialTreeFrame,
    int hollowTreeFoliageStyle,
    out int treeFrame)
  {
    treeFrame = initialTreeFrame;
    if (hollowTreeFoliageStyle != HollowTreeFoliageStyle)
    {
      treeFrame += (x % 3) switch
      {
        1 => 3,
        2 => 6,
        _ => 0
      };
      return treeFrame switch
      {
        0 => 919,
        1 => 918,
        2 => 924,
        3 => 921,
        4 => 922,
        5 => 923,
        6 => 920,
        7 => 925,
        8 => 917,
        _ => -1
      };
    }

    treeFrame += (x % 6) switch
    {
      1 => 3,
      2 => 6,
      3 => 9,
      4 => 12,
      5 => 15,
      _ => 0
    };
    return treeFrame switch
    {
      0 or 1 or 2 => 1113,
      3 or 5 => 1114,
      4 => 1115,
      6 => 1116,
      7 => 1117,
      8 => 1118,
      9 or 10 or 11 => 1119,
      12 or 13 or 14 => 1120,
      15 or 16 or 17 => 1121,
      _ => -1
    };
  }
}
