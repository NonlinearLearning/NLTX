using System.Buffers.Binary;

namespace Terraria.NonAuthoritative.Persistence;

public static class WorldTileHeaderCodec
{
  public static byte[] Encode(
    WorldTileHeaderCoreValue core,
    WorldTileHeaderExtensionValue extension)
  {
    bool hasHeader3 = extension.HasHeader3;
    bool hasHeader2 = core.RequiresHeader2 || hasHeader3;
    List<byte> bytes = new();
    bytes.Add(WorldTileHeaderCoreCodec.BuildHeader1(core, hasHeader2));
    if (hasHeader2)
    {
      bytes.Add(WorldTileHeaderCoreCodec.BuildHeader2(core, hasHeader3));
      if (hasHeader3)
      {
        bytes.Add(WorldTileHeaderExtensionCodec.BuildHeader3(extension));
        if (extension.HasHeader4)
        {
          bytes.Add(WorldTileHeaderExtensionCodec.BuildHeader4(extension));
        }
      }
    }

    if (core.Active)
    {
      bytes.Add((byte)core.TileType);
      if (core.TileType > byte.MaxValue)
      {
        bytes.Add((byte)(core.TileType >> 8));
      }
    }

    if (core.Wall)
    {
      bytes.Add((byte)core.WallType);
      if (core.WallType > byte.MaxValue)
      {
        bytes.Add((byte)(core.WallType >> 8));
      }
    }

    if (core.Liquid != TileLiquidKind.None)
    {
      bytes.Add(core.LiquidAmount);
    }

    if (extension.TileColor != 0)
    {
      bytes.Add(extension.TileColor);
    }

    if (extension.WallColor != 0)
    {
      bytes.Add(extension.WallColor);
    }

    if (core.RunLength != 0)
    {
      if (core.RunLength <= byte.MaxValue)
      {
        bytes.Add((byte)core.RunLength);
      }
      else
      {
        Span<byte> run = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(run, core.RunLength);
        bytes.Add(run[0]);
        bytes.Add(run[1]);
      }
    }

    return bytes.ToArray();
  }

  public static WorldTileHeaderValue Decode(ReadOnlySpan<byte> bytes)
  {
    if (bytes.IsEmpty)
    {
      throw new FormatException("Tile header bytes are empty.");
    }

    int offset = 0;
    byte header1 = ReadByte(bytes, ref offset);
    byte header2 = 0;
    byte header3 = 0;
    byte header4 = 0;
    if ((header1 & WorldTileHeaderCoreCodec.Header1Continuation) != 0)
    {
      header2 = ReadByte(bytes, ref offset);
      if ((header2 & WorldTileHeaderCoreCodec.Header2Continuation) != 0)
      {
        header3 = ReadByte(bytes, ref offset);
        if ((header3 & WorldTileHeaderExtensionCodec.Header3Continuation) != 0)
        {
          header4 = ReadByte(bytes, ref offset);
        }
      }
    }

    bool active = (header1 & WorldTileHeaderCoreCodec.Header1Active) != 0;
    ushort tileType = active
      ? ReadUnsignedValue(bytes, ref offset, (header1 & WorldTileHeaderCoreCodec.Header1HighTile) != 0)
      : (ushort)0;
    bool wall = (header1 & WorldTileHeaderCoreCodec.Header1Wall) != 0;
    ushort wallType = wall
      ? ReadUnsignedValue(bytes, ref offset, (header2 & WorldTileHeaderCoreCodec.Header2HighWall) != 0)
      : (ushort)0;
    TileLiquidKind liquid = DecodeLiquid(header1);
    byte liquidAmount = liquid == TileLiquidKind.None ? (byte)0 : ReadByte(bytes, ref offset);
    byte tileColor = (header3 & WorldTileHeaderExtensionCodec.Header3TileColor) != 0
      ? ReadByte(bytes, ref offset)
      : (byte)0;
    byte wallColor = (header3 & WorldTileHeaderExtensionCodec.Header3WallColor) != 0
      ? ReadByte(bytes, ref offset)
      : (byte)0;
    ushort runLength = DecodeRunLength(header1, bytes, ref offset);

    if (offset != bytes.Length)
    {
      throw new FormatException("Tile header contains trailing bytes.");
    }

    WorldTileHeaderCoreValue core = new(
      active,
      tileType,
      wall,
      wallType,
      liquid,
      liquidAmount,
      (header2 & WorldTileHeaderCoreCodec.Header2Wire1) != 0,
      (header2 & WorldTileHeaderCoreCodec.Header2Wire2) != 0,
      (header2 & WorldTileHeaderCoreCodec.Header2Wire3) != 0,
      (byte)((header2 & WorldTileHeaderCoreCodec.Header2SlopeMask) >> 4),
      runLength);
    WorldTileHeaderExtensionValue extension = new(
      (header3 & WorldTileHeaderExtensionCodec.Header3Actuator) != 0,
      (header3 & WorldTileHeaderExtensionCodec.Header3Inactive) != 0,
      (header3 & WorldTileHeaderExtensionCodec.Header3Wire4) != 0,
      tileColor,
      wallColor,
      (header4 & WorldTileHeaderExtensionCodec.Header4InvisibleBlock) != 0,
      (header4 & WorldTileHeaderExtensionCodec.Header4InvisibleWall) != 0,
      (header4 & WorldTileHeaderExtensionCodec.Header4FullbrightBlock) != 0,
      (header4 & WorldTileHeaderExtensionCodec.Header4FullbrightWall) != 0,
      (header3 & WorldTileHeaderExtensionCodec.Header3Shimmer) != 0,
      (byte)(header4 & WorldTileHeaderExtensionCodec.Header4ReservedMask));
    return new WorldTileHeaderValue(core, extension);
  }

  private static byte ReadByte(ReadOnlySpan<byte> bytes, ref int offset)
  {
    if ((uint)offset >= (uint)bytes.Length)
    {
      throw new FormatException("Tile header is truncated.");
    }

    return bytes[offset++];
  }

  private static ushort ReadUnsignedValue(ReadOnlySpan<byte> bytes, ref int offset, bool hasHighByte)
  {
    byte low = ReadByte(bytes, ref offset);
    byte high = hasHighByte ? ReadByte(bytes, ref offset) : (byte)0;
    return (ushort)(low | (high << 8));
  }

  private static TileLiquidKind DecodeLiquid(byte header1)
  {
    return (byte)(header1 & 0x18) switch
    {
      0 => TileLiquidKind.None,
      8 => TileLiquidKind.Water,
      16 => TileLiquidKind.Lava,
      24 => TileLiquidKind.Honey,
      _ => throw new FormatException("Invalid liquid header bits.")
    };
  }

  private static ushort DecodeRunLength(byte header1, ReadOnlySpan<byte> bytes, ref int offset)
  {
    bool hasByte = (header1 & WorldTileHeaderCoreCodec.Header1RunByte) != 0;
    bool hasShort = (header1 & WorldTileHeaderCoreCodec.Header1RunShort) != 0;
    if (hasByte && hasShort)
    {
      throw new FormatException("Tile header selects two run-length encodings.");
    }

    if (hasByte)
    {
      byte value = ReadByte(bytes, ref offset);
      if (value == 0)
      {
        throw new FormatException("Tile header contains a zero one-byte run.");
      }

      return value;
    }

    if (hasShort)
    {
      byte low = ReadByte(bytes, ref offset);
      byte high = ReadByte(bytes, ref offset);
      ushort value = (ushort)(low | (high << 8));
      if (value <= byte.MaxValue)
      {
        throw new FormatException("Tile header uses a short run for a one-byte length.");
      }

      return value;
    }

    return 0;
  }
}
