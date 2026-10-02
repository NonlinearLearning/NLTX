namespace Terraria.NonAuthoritative.Persistence;

public static class WorldTileHeaderExtensionCodec
{
  public const byte Header3Continuation = 0x01;
  public const byte Header3Actuator = 0x02;
  public const byte Header3Inactive = 0x04;
  public const byte Header3TileColor = 0x08;
  public const byte Header3WallColor = 0x10;
  public const byte Header3Wire4 = 0x20;
  public const byte Header3HighWall = 0x40;
  public const byte Header3Shimmer = 0x80;

  public const byte Header4InvisibleBlock = 0x02;
  public const byte Header4InvisibleWall = 0x04;
  public const byte Header4FullbrightBlock = 0x08;
  public const byte Header4FullbrightWall = 0x10;
  public const byte Header4ReservedMask = 0xE0;

  public static byte BuildHeader3(WorldTileHeaderExtensionValue value)
  {
    byte header = value.HasHeader4 ? Header3Continuation : (byte)0;
    if (value.Actuator)
    {
      header |= Header3Actuator;
    }

    if (value.Inactive)
    {
      header |= Header3Inactive;
    }

    if (value.TileColor != 0)
    {
      header |= Header3TileColor;
    }

    if (value.WallColor != 0)
    {
      header |= Header3WallColor;
    }

    if (value.Wire4)
    {
      header |= Header3Wire4;
    }

    if (value.Shimmer)
    {
      header |= Header3Shimmer;
    }

    return header;
  }

  public static byte BuildHeader4(WorldTileHeaderExtensionValue value)
  {
    byte header = value.ReservedHeader4Bits;
    if (value.InvisibleBlock)
    {
      header |= Header4InvisibleBlock;
    }

    if (value.InvisibleWall)
    {
      header |= Header4InvisibleWall;
    }

    if (value.FullbrightBlock)
    {
      header |= Header4FullbrightBlock;
    }

    if (value.FullbrightWall)
    {
      header |= Header4FullbrightWall;
    }

    return header;
  }

  public static byte[] Encode(WorldTileHeaderExtensionValue value)
  {
    if (!value.HasHeader3)
    {
      return Array.Empty<byte>();
    }

    return value.HasHeader4
      ? new[] { BuildHeader3(value), BuildHeader4(value) }
      : new[] { BuildHeader3(value) };
  }
}
