namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class MapEncodingCatalogComponent
{
  private MapEncodingCatalogComponent()
  {
  }

  public int DrawLoopMilliseconds { get; private init; }

  public int HeaderEmpty { get; private init; }

  public int HeaderTile { get; private init; }

  public int HeaderWall { get; private init; }

  public int HeaderWater { get; private init; }

  public int HeaderLava { get; private init; }

  public int HeaderHoney { get; private init; }

  public int HeaderHeavenAndHell { get; private init; }

  public int HeaderBackground { get; private init; }

  public int Header2ReadHeader3Bit { get; private init; }

  public int Header2Color1 { get; private init; }

  public int Header2Color2 { get; private init; }

  public int Header2Color3 { get; private init; }

  public int Header2Color4 { get; private init; }

  public int Header2Color5 { get; private init; }

  public int Header2ShimmerBit { get; private init; }

  public int Header2UnusedBit8 { get; private init; }

  public int Header3ReservedForHeader4Bit { get; private init; }

  public int Header3UnusedBit2 { get; private init; }

  public int Header3UnusedBit3 { get; private init; }

  public int Header3UnusedBit4 { get; private init; }

  public int Header3UnusedBit5 { get; private init; }

  public int Header3UnusedBit6 { get; private init; }

  public int Header3UnusedBit7 { get; private init; }

  public int Header3UnusedBit8 { get; private init; }

  public int MaxTileOptions { get; private init; }

  public int MaxWallOptions { get; private init; }

  public int MaxLiquidTypes { get; private init; }

  public int MaxSkyGradients { get; private init; }

  public int MaxDirtGradients { get; private init; }

  public int MaxRockGradients { get; private init; }

  public int MapChunkSize { get; private init; }

  public static MapEncodingCatalogComponent CreateDefault()
  {
    return new MapEncodingCatalogComponent
    {
      DrawLoopMilliseconds = 5,
      HeaderEmpty = 0,
      HeaderTile = 1,
      HeaderWall = 2,
      HeaderWater = 3,
      HeaderLava = 4,
      HeaderHoney = 5,
      HeaderHeavenAndHell = 6,
      HeaderBackground = 7,
      Header2ReadHeader3Bit = 1,
      Header2Color1 = 2,
      Header2Color2 = 4,
      Header2Color3 = 8,
      Header2Color4 = 16,
      Header2Color5 = 32,
      Header2ShimmerBit = 64,
      Header2UnusedBit8 = 128,
      Header3ReservedForHeader4Bit = 1,
      Header3UnusedBit2 = 2,
      Header3UnusedBit3 = 4,
      Header3UnusedBit4 = 8,
      Header3UnusedBit5 = 16,
      Header3UnusedBit6 = 32,
      Header3UnusedBit7 = 64,
      Header3UnusedBit8 = 128,
      MaxTileOptions = 13,
      MaxWallOptions = 2,
      MaxLiquidTypes = 4,
      MaxSkyGradients = 256,
      MaxDirtGradients = 256,
      MaxRockGradients = 256,
      MapChunkSize = 64
    };
  }
}
