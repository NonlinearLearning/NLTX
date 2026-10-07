using System.IO;
using System.Threading;
using NSSLC.WorldGeneration;
using Terraria.NonAuthoritative.Persistence;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

/// <summary>
/// WorldFile 319 tile headers and vertical run-length encoding; no runtime globals.
/// </summary>
public sealed class WorldFileTileCodec : IWorldTilePayloadDecoder {
  private const int MaxTileCount = 25_000_000;

  public WorldFileTilePayloadSection Encode(GeneratedWorld world) {
    ArgumentNullException.ThrowIfNull(world);
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    for (int x = 0; x < world.Width; x++) {
      for (int y = 0; y < world.Height; y++) {
        TileCellState tile = Normalize(world.GetTile(x, y), world.FrameImportant);
        int repeat = 0;
        if (world.CompressionBatching[tile.Type]) {
          while (y + repeat + 1 < world.Height && repeat < short.MaxValue &&
              tile.Equals(Normalize(world.GetTile(x, y + repeat + 1), world.FrameImportant))) {
            repeat++;
          }
        }
        WriteTile(writer, tile, world.FrameImportant, repeat);
        y += repeat;
      }
      if (stream.Length > WorldFileFormatConstants.MaxRawSectionBytes) {
        throw new InvalidDataException("The compressed tile section exceeds the WorldFile limit.");
      }
    }
    return new WorldFileTilePayloadSection(world.Width, world.Height,
        world.FrameImportant, stream.ToArray());
  }

  public WorldFileTilePayloadSection Encode(
      TileMapSnapshot tiles,
      IReadOnlyList<bool> frameImportant,
      CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentNullException.ThrowIfNull(frameImportant);
    if (tiles.Width <= 0 || tiles.Height <= 0 ||
        (long)tiles.Width * tiles.Height > MaxTileCount ||
        frameImportant.Count == 0 || frameImportant.Count > ushort.MaxValue) {
      throw new InvalidDataException("The runtime tile map dimensions or importance table are invalid.");
    }

    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    for (int x = 0; x < tiles.Width; x++) {
      cancellationToken.ThrowIfCancellationRequested();
      for (int y = 0; y < tiles.Height; y++) {
        if ((y & 255) == 0) {
          cancellationToken.ThrowIfCancellationRequested();
        }
        TileCellState tile = Normalize(tiles.GetTile(x, y), frameImportant);
        int repeat = 0;
        while (y + repeat + 1 < tiles.Height && repeat < short.MaxValue &&
            tile.Equals(Normalize(tiles.GetTile(x, y + repeat + 1), frameImportant))) {
          repeat++;
        }
        WriteTile(writer, tile, frameImportant, repeat);
        y += repeat;
      }
      if (stream.Length > WorldFileFormatConstants.MaxRawSectionBytes) {
        throw new InvalidDataException("The compressed runtime tile section exceeds the WorldFile limit.");
      }
    }

    return new WorldFileTilePayloadSection(
        tiles.Width, tiles.Height, frameImportant, stream.ToArray());
  }

  public TileMapSnapshot Decode(WorldFileTilePayloadSection section) {
    ArgumentNullException.ThrowIfNull(section);
    long count = (long)section.MaxTilesX * section.MaxTilesY;
    if (section.MaxTilesX <= 0 || section.MaxTilesY <= 0 || count > MaxTileCount ||
        section.FrameImportant.Count == 0 || section.CompressedPayload.IsEmpty ||
        section.CompressedPayload.Length > WorldFileFormatConstants.MaxRawSectionBytes) {
      throw new InvalidDataException(
          "The tile dimensions, importance table or payload are invalid.");
    }
    var tiles = new TileCellState[(int)count];
    using var stream = new MemoryStream(section.CompressedPayload.ToArray(), writable: false);
    using var reader = new BinaryReader(stream);
    for (int x = 0; x < section.MaxTilesX; x++) {
      for (int y = 0; y < section.MaxTilesY; y++) {
        TileCellState tile = ReadTile(reader, section.FrameImportant, out int repeat);
        if (repeat < 0 || repeat >= section.MaxTilesY - y) {
          throw new InvalidDataException("A tile run crosses a column boundary.");
        }
        Array.Fill(tiles, tile, x * section.MaxTilesY + y, repeat + 1);
        y += repeat;
      }
    }
    if (stream.Position != stream.Length) {
      throw new InvalidDataException("The tile payload has trailing bytes.");
    }
    return new TileMapSnapshot(section.MaxTilesX, section.MaxTilesY, tiles);
  }

  /// <summary>
  /// Only persisted facts participate; inactive type/frame and framing caches do not.
  /// </summary>
  public static TileCellState Normalize(GeneratedTile tile, IReadOnlyList<bool> importance) {
    if (tile.Active && tile.Type >= importance.Count) {
      throw new InvalidDataException("An active tile is outside the frame importance table.");
    }
    ushort header = tile.Active ? tile.TileHeader : (ushort)(tile.TileHeader & 0xffe0);
    if ((header & 0x400) != 0) {
      header &= 0x8fff;
    }
    int slope = (header >> 12) & 7;
    if (slope > 4) {
      throw new InvalidDataException("A tile slope is outside the supported range.");
    }
    bool framed = tile.Active && importance[tile.Type];
    byte liquidType = tile.Liquid == 0 ? (byte)0 : (byte)tile.LiquidType;
    return new TileCellState {
      Type = tile.Active ? tile.Type : (ushort)0,
      Wall = tile.Wall,
      FrameX = framed ? tile.FrameX : tile.Active ? (short)-1 : (short)0,
      FrameY = framed ? tile.FrameY : tile.Active ? (short)-1 : (short)0,
      TileHeader = header,
      Header = (byte)((tile.Header & 0x80) | (tile.Wall > 0 ? tile.Header & 31 : 0) |
          (liquidType << 5)),
      Header3 = (byte)(tile.Header3 & 0xe0),
      LiquidAmount = tile.Liquid,
      LiquidType = liquidType
    };
  }

  private static TileCellState Normalize(TileCellState tile, IReadOnlyList<bool> importance) {
    bool active = (tile.TileHeader & 32) != 0;
    if (active && tile.Type >= importance.Count) {
      throw new InvalidDataException("An active runtime tile is outside the frame importance table.");
    }
    ushort header = active ? tile.TileHeader : (ushort)(tile.TileHeader & 0xffe0);
    if ((header & 0x400) != 0) {
      header &= 0x8fff;
    }
    if (((header >> 12) & 7) > 4) {
      throw new InvalidDataException("A runtime tile slope is outside the supported range.");
    }
    bool framed = active && importance[tile.Type];
    byte liquidType = tile.LiquidAmount == 0 ? (byte)0 : (byte)tile.LiquidType;
    return new TileCellState {
      Type = active ? tile.Type : (ushort)0,
      Wall = tile.Wall,
      FrameX = framed ? tile.FrameX : active ? (short)-1 : (short)0,
      FrameY = framed ? tile.FrameY : active ? (short)-1 : (short)0,
      TileHeader = header,
      Header = (byte)((tile.Header & 0x80) | (tile.Wall > 0 ? tile.Header & 31 : 0) |
          (liquidType << 5)),
      Header3 = (byte)(tile.Header3 & 0xe0),
      LiquidAmount = tile.LiquidAmount,
      LiquidType = liquidType,
    };
  }

  private static void WriteTile(BinaryWriter writer, TileCellState tile,
      IReadOnlyList<bool> importance, int repeat) {
    byte first = 0;
    byte second = 0;
    byte third = 0;
    byte fourth = 0;
    bool active = (tile.TileHeader & 32) != 0;
    if (active) {
      first |= 2;
      if (tile.Type > 255) {
        first |= 32;
      }
      if ((tile.TileHeader & 31) != 0) {
        third |= 8;
      }
    }
    if (tile.Wall != 0) {
      first |= 4;
      if ((tile.Header & 31) != 0) {
        third |= 16;
      }
      if (tile.Wall > 255) {
        third |= 64;
      }
    }
    if (tile.LiquidAmount != 0) {
      first |= tile.LiquidType switch { 1 => (byte)16, 2 => (byte)24, _ => (byte)8 };
      if (tile.LiquidType == 3) {
        third |= 128;
      }
    }
    second |= (byte)((tile.TileHeader & 0x380) >> 6);
    int shape = (tile.TileHeader & 0x400) != 0 ? 1 : (tile.TileHeader >> 12) & 7;
    if ((tile.TileHeader & 0x400) == 0 && shape != 0) {
      shape++;
    }
    second |= (byte)(shape << 4);
    third |= (byte)((tile.TileHeader & 0x800) >> 10);
    third |= (byte)((tile.TileHeader & 0x40) >> 4);
    third |= (byte)((tile.Header & 0x80) >> 2);
    fourth |= (byte)((tile.Header3 & 0xe0) >> 4);
    fourth |= (byte)((tile.TileHeader & 0x8000) >> 11);
    if (fourth != 0) {
      third |= 1;
    }
    if (third != 0) {
      second |= 1;
    }
    if (second != 0) {
      first |= 1;
    }
    first |= repeat > 255 ? (byte)128 : repeat > 0 ? (byte)64 : (byte)0;
    writer.Write(first);
    if (second != 0) {
      writer.Write(second);
    }
    if (third != 0) {
      writer.Write(third);
    }
    if (fourth != 0) {
      writer.Write(fourth);
    }
    if (active) {
      writer.Write((byte)tile.Type);
      if (tile.Type > 255) {
        writer.Write((byte)(tile.Type >> 8));
      }
      if (importance[tile.Type]) {
        writer.Write(tile.FrameX);
        writer.Write(tile.FrameY);
      }
      if ((third & 8) != 0) {
        writer.Write((byte)(tile.TileHeader & 31));
      }
    }
    if (tile.Wall != 0) {
      writer.Write((byte)tile.Wall);
      if ((third & 16) != 0) {
        writer.Write((byte)(tile.Header & 31));
      }
    }
    if (tile.LiquidAmount != 0) {
      writer.Write(tile.LiquidAmount);
    }
    if ((third & 64) != 0) {
      writer.Write((byte)(tile.Wall >> 8));
    }
    if (repeat > 255) {
      writer.Write((short)repeat);
    } else if (repeat > 0) {
      writer.Write((byte)repeat);
    }
  }

  private static TileCellState ReadTile(BinaryReader reader, IReadOnlyList<bool> importance,
      out int repeat) {
    byte first = reader.ReadByte();
    byte second = (first & 1) != 0 ? reader.ReadByte() : (byte)0;
    byte third = (second & 1) != 0 ? reader.ReadByte() : (byte)0;
    byte fourth = (third & 1) != 0 ? reader.ReadByte() : (byte)0;
    if ((fourth & 0xe1) != 0) {
      throw new InvalidDataException("Unsupported tile header flags.");
    }
    var tile = new TileCellState();
    if ((first & 2) != 0) {
      tile.TileHeader = 32;
      tile.Type = reader.ReadByte();
      if ((first & 32) != 0) {
        tile.Type |= (ushort)(reader.ReadByte() << 8);
      }
      if (tile.Type >= importance.Count) {
        throw new InvalidDataException("A tile type is outside the importance table.");
      }
      tile.FrameX = importance[tile.Type] ? reader.ReadInt16() : (short)-1;
      tile.FrameY = importance[tile.Type] ? reader.ReadInt16() : (short)-1;
      if ((third & 8) != 0) {
        tile.TileHeader |= ReadPaint(reader);
      }
    } else if ((first & 32) != 0 || (third & 8) != 0) {
      throw new InvalidDataException("An inactive tile carries active-only fields.");
    }
    if ((first & 4) != 0) {
      tile.Wall = reader.ReadByte();
      if ((third & 16) != 0) {
        tile.Header = ReadPaint(reader);
      }
    } else if ((third & 80) != 0) {
      throw new InvalidDataException("A missing wall carries wall-only fields.");
    }
    int liquid = (first >> 3) & 3;
    if (liquid != 0) {
      tile.LiquidAmount = reader.ReadByte();
      if (tile.LiquidAmount == 0) {
        throw new InvalidDataException("A present liquid has zero amount.");
      }
      tile.LiquidType = (third & 128) != 0 ? (byte)3 : (byte)(liquid - 1);
      tile.Header |= (byte)(tile.LiquidType << 5);
    } else if ((third & 128) != 0) {
      throw new InvalidDataException("Shimmer is flagged without liquid.");
    }
    tile.TileHeader |= (ushort)((second & 14) << 6);
    int shape = (second >> 4) & 7;
    if (shape > 5) {
      throw new InvalidDataException("An invalid tile slope was encoded.");
    }
    tile.TileHeader |= shape == 1 ? (ushort)0x400 :
        shape > 1 ? (ushort)((shape - 1) << 12) : (ushort)0;
    tile.TileHeader |= (ushort)((third & 2) << 10);
    tile.TileHeader |= (ushort)((third & 4) << 4);
    tile.Header |= (byte)((third & 32) << 2);
    if ((third & 64) != 0) {
      tile.Wall |= (ushort)(reader.ReadByte() << 8);
    }
    tile.Header3 = (byte)((fourth & 14) << 4);
    tile.TileHeader |= (ushort)((fourth & 16) << 11);
    repeat = (first >> 6) switch {
      0 => 0,
      1 => reader.ReadByte(),
      2 => reader.ReadInt16(),
      _ => throw new InvalidDataException("Reserved tile run length header.")
    };
    return tile;
  }

  private static byte ReadPaint(BinaryReader reader) {
    byte paint = reader.ReadByte();
    if (paint > 31) {
      throw new InvalidDataException("A tile paint value is outside its five-bit field.");
    }
    return paint;
  }
}
