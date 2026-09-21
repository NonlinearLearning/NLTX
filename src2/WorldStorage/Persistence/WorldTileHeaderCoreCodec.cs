namespace Terraria.NonAuthoritative.Persistence;

public static class WorldTileHeaderCoreCodec
{
  public const byte Header1Continuation = 0x01;
  public const byte Header1Active = 0x02;
  public const byte Header1Wall = 0x04;
  public const byte Header1LiquidWater = 0x08;
  public const byte Header1LiquidLava = 0x10;
  public const byte Header1LiquidHoney = 0x18;
  public const byte Header1HighTile = 0x20;
  public const byte Header1RunByte = 0x40;
  public const byte Header1RunShort = 0x80;

  public const byte Header2Continuation = 0x01;
  public const byte Header2Wire1 = 0x02;
  public const byte Header2Wire2 = 0x04;
  public const byte Header2Wire3 = 0x08;
  public const byte Header2SlopeMask = 0x70;
  public const byte Header2HighWall = 0x40;
  public const byte Header2LegacyShimmer = 0x80;

  public static byte BuildHeader1(WorldTileHeaderCoreValue value, bool hasHeader2)
  {
    byte header = hasHeader2 ? Header1Continuation : (byte)0;
    if (value.Active)
    {
      header |= Header1Active;
    }

    if (value.Wall)
    {
      header |= Header1Wall;
    }

    header |= value.Liquid switch
    {
      TileLiquidKind.None => (byte)0,
      TileLiquidKind.Water => Header1LiquidWater,
      TileLiquidKind.Lava => Header1LiquidLava,
      TileLiquidKind.Honey => Header1LiquidHoney,
      _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    if (value.Active && value.TileType > byte.MaxValue)
    {
      header |= Header1HighTile;
    }

    if (value.RunLength != 0)
    {
      header |= value.RunLength <= byte.MaxValue ? Header1RunByte : Header1RunShort;
    }

    return header;
  }

  public static byte BuildHeader2(WorldTileHeaderCoreValue value, bool hasHeader3)
  {
    byte header = hasHeader3 ? Header2Continuation : (byte)0;
    if (value.Wire1)
    {
      header |= Header2Wire1;
    }

    if (value.Wire2)
    {
      header |= Header2Wire2;
    }

    if (value.Wire3)
    {
      header |= Header2Wire3;
    }

    header |= (byte)((value.Slope & 0x07) << 4);
    if (value.Wall && value.WallType > byte.MaxValue)
    {
      header |= Header2HighWall;
    }

    return header;
  }

  public static byte[] Encode(WorldTileHeaderCoreValue value, bool hasHeader3)
  {
    bool hasHeader2 = value.RequiresHeader2 || hasHeader3;
    return hasHeader2
      ? new[] { BuildHeader1(value, hasHeader2), BuildHeader2(value, hasHeader3) }
      : new[] { BuildHeader1(value, false) };
  }
}
