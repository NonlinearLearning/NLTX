using System.IO;
using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.World;

/// <summary>Decodes the Version4 .wld vertical tile runs into the P03 row-major tile snapshot.</summary>
public sealed class WorldFileTilePayloadDecoder
{
  private const int _timerTileType = 144;
  private const int MaximumTileCount = 25_000_000;
  private const int MaximumPayloadBytes = 64 * 1024 * 1024;

  public IReadOnlyList<WorldTile> Decode(
    WorldFileTilePayloadSection section,
    WorldFacts facts,
    long maximumSnapshotBytes = 1_073_741_824)
  {
    ArgumentNullException.ThrowIfNull(section);
    ArgumentNullException.ThrowIfNull(facts);
    if (maximumSnapshotBytes <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumSnapshotBytes));
    }

    long tileCount = (long)section.MaxTilesX * section.MaxTilesY;
    if (section.MaxTilesX <= 0 || section.MaxTilesY <= 0 || tileCount > MaximumTileCount ||
        tileCount > maximumSnapshotBytes / 32 || section.CompressedPayload.IsEmpty ||
        section.CompressedPayload.Length > MaximumPayloadBytes ||
        section.FrameImportant.Count != WorldFacts.TileCount)
    {
      throw new InvalidDataException("The tile dimensions, facts or payload exceed P03 limits.");
    }

    for (int index = 0; index < WorldFacts.TileCount; index++)
    {
      if (section.FrameImportant[index] != facts.FrameImportant[index])
      {
        throw new InvalidDataException("The .wld tile payload uses different frame-importance facts.");
      }
    }

    var tiles = new WorldTile[(int)tileCount];
    using var stream = new MemoryStream(section.CompressedPayload.ToArray(), writable: false);
    using var reader = new BinaryReader(stream);
    for (int x = 0; x < section.MaxTilesX; x++)
    {
      for (int y = 0; y < section.MaxTilesY; y++)
      {
        WorldTile tile = ReadTile(reader, facts, out int repeat);
        if (repeat < 0 || repeat >= section.MaxTilesY - y)
        {
          throw new InvalidDataException("A tile run crosses a world column boundary.");
        }

        for (int offset = 0; offset <= repeat; offset++)
        {
          tiles[((y + offset) * section.MaxTilesX) + x] = tile;
        }

        y += repeat;
      }
    }

    if (stream.Position != stream.Length)
    {
      throw new InvalidDataException("The tile payload contains trailing bytes.");
    }

    return Array.AsReadOnly(tiles);
  }

  private static WorldTile ReadTile(BinaryReader reader, WorldFacts facts, out int repeat)
  {
    byte first = reader.ReadByte();
    byte second = (first & 1) != 0 ? reader.ReadByte() : (byte)0;
    byte third = (second & 1) != 0 ? reader.ReadByte() : (byte)0;
    byte fourth = (third & 1) != 0 ? reader.ReadByte() : (byte)0;
    if ((fourth & 0xe1) != 0)
    {
      throw new InvalidDataException("The tile payload contains unsupported header flags.");
    }

    bool active = (first & 2) != 0;
    ushort type = 0;
    short frameX = 0;
    short frameY = 0;
    byte tileColor = 0;
    if (active)
    {
      type = reader.ReadByte();
      if ((first & 32) != 0)
      {
        type |= (ushort)(reader.ReadByte() << 8);
      }

      if (!facts.IsValidTileType(type))
      {
        throw new InvalidDataException("An active tile type is outside the Version4 facts table.");
      }

      if (facts.FrameImportant[type])
      {
        frameX = reader.ReadInt16();
        frameY = reader.ReadInt16();
        if (type == _timerTileType) {
          frameY = 0;
        }
      }
      else
      {
        frameX = -1;
        frameY = -1;
      }

      if ((third & 8) != 0)
      {
        tileColor = ReadPaint(reader);
      }
    }
    else if ((first & 32) != 0 || (third & 8) != 0)
    {
      throw new InvalidDataException("An inactive tile carries active-only fields.");
    }

    ushort wall = 0;
    byte wallColor = 0;
    if ((first & 4) != 0)
    {
      wall = reader.ReadByte();
      if ((third & 16) != 0)
      {
        wallColor = ReadPaint(reader);
      }
    }
    else if ((third & 80) != 0)
    {
      throw new InvalidDataException("A missing wall carries wall-only fields.");
    }

    byte liquidAmount = 0;
    LiquidType liquidType = LiquidType.Water;
    int liquidKind = (first >> 3) & 3;
    if (liquidKind != 0)
    {
      liquidAmount = reader.ReadByte();
      if (liquidAmount == 0)
      {
        throw new InvalidDataException("A present liquid has zero amount.");
      }

      liquidType = (third & 128) != 0
        ? LiquidType.Shimmer
        : (LiquidType)(liquidKind - 1);
    }
    else if ((third & 128) != 0)
    {
      throw new InvalidDataException("Shimmer is flagged without liquid.");
    }

    if ((third & 64) != 0)
    {
      wall |= (ushort)(reader.ReadByte() << 8);
    }

    if (wall >= WorldFacts.WallCount) {
      wall = 0;
    }

    TileWireMask wires = (TileWireMask)(((second & 14) >> 1) | ((third & 32) >> 2));
    int shape = (second >> 4) & 7;
    if (shape > 5)
    {
      throw new InvalidDataException("The tile payload contains an invalid slope.");
    }

    if (!facts.SaveSlopes[type]) {
      shape = 0;
    }

    bool halfBlock = shape == 1;
    byte slope = shape > 1 ? (byte)(shape - 1) : (byte)0;
    byte tileHeader3 = (byte)((fourth & 14) << 4);
    byte wallHeader = (byte)((fourth & 16) << 3);
    repeat = (first >> 6) switch
    {
      0 => 0,
      1 => reader.ReadByte(),
      2 => reader.ReadInt16(),
      _ => throw new InvalidDataException("The tile run uses a reserved header value.")
    };

    return new WorldTile(
      active,
      type,
      wall,
      frameX,
      frameY,
      liquidAmount,
      liquidType,
      tileColor,
      wallColor,
      wires,
      (third & 2) != 0,
      (third & 4) != 0,
      halfBlock,
      slope,
      tileHeader3,
      wallHeader);
  }

  private static byte ReadPaint(BinaryReader reader)
  {
    byte paint = reader.ReadByte();
    if (paint > 31)
    {
      throw new InvalidDataException("A paint value exceeds the five-bit tile field.");
    }

    return paint;
  }
}
