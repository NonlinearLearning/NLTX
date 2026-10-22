using System.IO;
using System.Threading;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Server.World;

/// <summary>
/// Encodes an authoritative P03 tile snapshot as the Version4 vertical tile stream.
/// </summary>
public sealed class WorldFileTilePayloadEncoder
{
  private const int _timerTileType = 144;
  private const int _maximumPayloadBytes = 64 * 1024 * 1024;

  public WorldFileTilePayloadSection Encode(
    WorldGeometry geometry,
    IReadOnlyList<WorldTile> tiles,
    WorldFacts facts,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentNullException.ThrowIfNull(facts);
    ValidateInput(geometry, tiles, facts);
    cancellationToken.ThrowIfCancellationRequested();

    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    for (int x = 0; x < geometry.Width; x++)
    {
      cancellationToken.ThrowIfCancellationRequested();
      for (int y = 0; y < geometry.Height; y++)
      {
        if ((y & 255) == 0)
        {
          cancellationToken.ThrowIfCancellationRequested();
        }

        WorldTile sourceTile = tiles[(y * geometry.Width) + x];
        WorldTile tile = Normalize(sourceTile, facts);
        int repeat = 0;
        if (facts.AllowsSaveCompressionBatching[sourceTile.Type])
        {
          while (y + repeat + 1 < geometry.Height && repeat < short.MaxValue)
          {
            if ((repeat & 255) == 255)
            {
              cancellationToken.ThrowIfCancellationRequested();
            }

            WorldTile next = Normalize(
              tiles[((y + repeat + 1) * geometry.Width) + x],
              facts);
            if (next != tile)
            {
              break;
            }

            repeat++;
          }
        }

        WriteTile(writer, tile, facts.FrameImportant, repeat);
        if (stream.Length > _maximumPayloadBytes)
        {
          throw new InvalidDataException("The compressed tile section exceeds the P03 limit.");
        }

        y += repeat;
      }
    }

    cancellationToken.ThrowIfCancellationRequested();
    return new WorldFileTilePayloadSection(
      geometry.Width,
      geometry.Height,
      facts.FrameImportant,
      stream.ToArray());
  }

  public WorldFileTilePayloadSection EncodeEmpty(
    WorldGeometry geometry,
    WorldFacts facts,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(facts);
    long tileCount = (long)geometry.Width * geometry.Height;
    if (geometry.Width <= 0 || geometry.Height <= 0 ||
        tileCount > WorldGeometry.MaximumTileCount ||
        facts.FrameImportant.Count != WorldFacts.TileCount ||
        facts.AllowsSaveCompressionBatching.Count != WorldFacts.TileCount ||
        facts.SaveSlopes.Count != WorldFacts.TileCount)
    {
      throw new InvalidDataException("The empty tile dimensions or facts are invalid.");
    }

    cancellationToken.ThrowIfCancellationRequested();

    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    WorldTile emptyTile = Normalize(WorldTile.Empty, facts);
    for (int x = 0; x < geometry.Width; x++)
    {
      cancellationToken.ThrowIfCancellationRequested();
      int rowsRemaining = geometry.Height;
      while (rowsRemaining > 0)
      {
        int repeat = Math.Min(rowsRemaining - 1, short.MaxValue);
        WriteTile(writer, emptyTile, facts.FrameImportant, repeat);
        rowsRemaining -= repeat + 1;
        if (stream.Length > _maximumPayloadBytes)
        {
          throw new InvalidDataException("The compressed tile section exceeds the P03 limit.");
        }
      }
    }

    cancellationToken.ThrowIfCancellationRequested();
    return new WorldFileTilePayloadSection(
      geometry.Width,
      geometry.Height,
      facts.FrameImportant,
      stream.ToArray());
  }

  private static void ValidateInput(
    WorldGeometry geometry,
    IReadOnlyList<WorldTile> tiles,
    WorldFacts facts)
  {
    long tileCount = (long)geometry.Width * geometry.Height;
    if (geometry.Width <= 0 || geometry.Height <= 0 ||
        tileCount > WorldGeometry.MaximumTileCount || tileCount != tiles.Count ||
        facts.FrameImportant.Count != WorldFacts.TileCount ||
        facts.AllowsSaveCompressionBatching.Count != WorldFacts.TileCount ||
        facts.SaveSlopes.Count != WorldFacts.TileCount)
    {
      throw new InvalidDataException("The runtime tile dimensions, snapshot or facts are invalid.");
    }
  }

  private static WorldTile Normalize(WorldTile tile, WorldFacts facts)
  {
    if (!facts.IsValidTileType(tile.Type) ||
        tile.Wall >= WorldFacts.WallCount ||
        !Enum.IsDefined(tile.LiquidType) ||
        (((byte)tile.Wires) & 0xf0) != 0 ||
        tile.Slope > 4 ||
        tile.TileColor > 31 ||
        tile.WallColor > 31 ||
        (tile.TileHeader3 & 0x1f) != 0 ||
        (tile.WallHeader & 0x7f) != 0 ||
        (tile.HalfBlock && tile.Slope != 0))
    {
      throw new InvalidDataException("A runtime tile contains fields outside the Version4 facts.");
    }

    ushort type = tile.Active ? tile.Type : (ushort)0;
    short frameX = tile.Active ? tile.FrameX : (short)0;
    short frameY = tile.Active ? tile.FrameY : (short)0;
    if (tile.Active && !facts.FrameImportant[type])
    {
      frameX = -1;
      frameY = -1;
    }
    else if (type == _timerTileType)
    {
      frameY = 0;
    }

    bool halfBlock = tile.HalfBlock;
    byte slope = tile.Slope;
    if (!facts.SaveSlopes[type])
    {
      halfBlock = false;
      slope = 0;
    }

    return tile with
    {
      Type = type,
      FrameX = frameX,
      FrameY = frameY,
      LiquidType = tile.LiquidAmount == 0 ? LiquidType.Water : tile.LiquidType,
      TileColor = tile.Active ? tile.TileColor : (byte)0,
      WallColor = tile.Wall == 0 ? (byte)0 : tile.WallColor,
      HalfBlock = halfBlock,
      Slope = slope
    };
  }

  private static void WriteTile(
    BinaryWriter writer,
    WorldTile tile,
    IReadOnlyList<bool> frameImportant,
    int repeat)
  {
    byte first = 0;
    byte second = (byte)(((byte)tile.Wires & 0x07) << 1);
    byte third = (byte)(((byte)tile.Wires & 0x08) << 2);
    byte fourth = (byte)(((tile.TileHeader3 & 0xe0) >> 4) | ((tile.WallHeader & 0x80) >> 3));

    if (tile.Active)
    {
      first |= 2;
      if (tile.Type > byte.MaxValue)
      {
        first |= 32;
      }

      if (tile.TileColor != 0)
      {
        third |= 8;
      }
    }

    if (tile.Wall != 0)
    {
      first |= 4;
      if (tile.WallColor != 0)
      {
        third |= 16;
      }

      if (tile.Wall > byte.MaxValue)
      {
        third |= 64;
      }
    }

    if (tile.LiquidAmount != 0)
    {
      first |= tile.LiquidType switch
      {
        LiquidType.Water => 8,
        LiquidType.Lava => 16,
        LiquidType.Honey => 24,
        LiquidType.Shimmer => 8,
        _ => throw new InvalidDataException("The runtime liquid type is invalid.")
      };
      if (tile.LiquidType == LiquidType.Shimmer)
      {
        third |= 128;
      }
    }

    if (tile.Actuator)
    {
      third |= 2;
    }

    if (tile.Inactive)
    {
      third |= 4;
    }

    int shape = tile.HalfBlock ? 1 : tile.Slope == 0 ? 0 : tile.Slope + 1;
    second |= (byte)(shape << 4);
    if (fourth != 0)
    {
      third |= 1;
    }

    if (third != 0)
    {
      second |= 1;
    }

    if (second != 0)
    {
      first |= 1;
    }

    first |= repeat > byte.MaxValue ? (byte)128 : repeat > 0 ? (byte)64 : (byte)0;
    writer.Write(first);
    if ((first & 1) != 0)
    {
      writer.Write(second);
      if ((second & 1) != 0)
      {
        writer.Write(third);
        if ((third & 1) != 0)
        {
          writer.Write(fourth);
        }
      }
    }

    if (tile.Active)
    {
      writer.Write((byte)tile.Type);
      if (tile.Type > byte.MaxValue)
      {
        writer.Write((byte)(tile.Type >> 8));
      }

      if (frameImportant[tile.Type])
      {
        writer.Write(tile.FrameX);
        writer.Write(tile.FrameY);
      }

      if ((third & 8) != 0)
      {
        writer.Write(tile.TileColor);
      }
    }

    if (tile.Wall != 0)
    {
      writer.Write((byte)tile.Wall);
      if ((third & 16) != 0)
      {
        writer.Write(tile.WallColor);
      }
    }

    if (tile.LiquidAmount != 0)
    {
      writer.Write(tile.LiquidAmount);
    }

    if ((third & 64) != 0)
    {
      writer.Write((byte)(tile.Wall >> 8));
    }

    if (repeat > byte.MaxValue)
    {
      writer.Write((short)repeat);
    }
    else if (repeat > 0)
    {
      writer.Write((byte)repeat);
    }
  }
}
