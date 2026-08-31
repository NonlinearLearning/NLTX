using System;
using System.IO;
using System.Text;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Persistence;

public static class WorldPersistenceFormat
{
  private const int FormatVersion = 8;
  private const int NoTrapsWorldFormatVersion = 6;
  private const int SkyblockWorldFormatVersion = 7;
  private const int GoodWorldFormatVersion = 8;
  private const int RemixWorldFormatVersion = 5;
  private const int WorldSurfaceFormatVersion = 3;
  private const int RockLayerFormatVersion = 5;
  private const int LegacyFormatVersion = 1;
  private const int PreviousFormatVersion = 2;
  private const int Magic = 0x574D4F44;
  private const int MaximumWorldNameBytes = 1024;
  private const int MaximumWorldTileCount = 10_080_000;

  public static WorldGridSnapshot Read(Stream input)
  {
    ArgumentNullException.ThrowIfNull(input);
    using BinaryReader reader = new(input, Encoding.UTF8, leaveOpen: true);
    if (reader.ReadInt32() != Magic)
    {
      throw new InvalidDataException("The world file magic is not recognized.");
    }

    int formatVersion = reader.ReadInt32();
    if (formatVersion < LegacyFormatVersion || formatVersion > FormatVersion)
    {
      throw new InvalidDataException("The world file format version is not supported.");
    }

    int width = reader.ReadInt32();
    int height = reader.ReadInt32();
    ValidateDimensions(width, height);
    WorldSeed seed = new(reader.ReadInt32());
    string name = ReadName(reader);
    double? worldSurface = ReadWorldSurface(reader, formatVersion);
    double? rockLayer = ReadRockLayer(reader, formatVersion);
    bool? isRemixWorld = ReadIsRemixWorld(reader, formatVersion);
    bool? isNoTrapsWorld = ReadIsNoTrapsWorld(reader, formatVersion);
    bool? isSkyblockWorld = ReadIsSkyblockWorld(reader, formatVersion);
    bool? isGoodWorld = ReadIsGoodWorld(reader, formatVersion);
    WorldMetadata metadata = new(
      name,
      seed,
      width,
      height,
      worldSurface: worldSurface,
      rockLayer: rockLayer,
      isRemixWorld: isRemixWorld,
      isNoTrapsWorld: isNoTrapsWorld,
      isSkyblockWorld: isSkyblockWorld,
      isGoodWorld: isGoodWorld);
    WorldTile[,] tiles = new WorldTile[width, height];
    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        tiles[x, y] = formatVersion == LegacyFormatVersion
          ? ReadLegacyTile(reader)
          : ReadTile(reader);
      }
    }

    int sectionCount = checked((width / WorldGrid.SectionWidth) *
      (height / WorldGrid.SectionHeight));
    if (reader.ReadInt32() != sectionCount)
    {
      throw new InvalidDataException("The world file section-version count is invalid.");
    }

    long[,] sectionVersions = new long[
      width / WorldGrid.SectionWidth,
      height / WorldGrid.SectionHeight];
    for (int y = 0; y < sectionVersions.GetLength(1); y++)
    {
      for (int x = 0; x < sectionVersions.GetLength(0); x++)
      {
        long sectionVersion = reader.ReadInt64();
        if (sectionVersion < 0)
        {
          throw new InvalidDataException("A section version cannot be negative.");
        }

        sectionVersions[x, y] = sectionVersion;
      }
    }

    if (input.ReadByte() != -1)
    {
      throw new InvalidDataException("The world file contains trailing data.");
    }

    return new WorldGridSnapshot(metadata, tiles, sectionVersions);
  }

  public static void Write(Stream output, WorldGridSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(output);
    ArgumentNullException.ThrowIfNull(snapshot);
    using BinaryWriter writer = new(output, Encoding.UTF8, leaveOpen: true);
    writer.Write(Magic);
    writer.Write(FormatVersion);
    writer.Write(snapshot.Metadata.Width);
    writer.Write(snapshot.Metadata.Height);
    writer.Write(snapshot.Metadata.Seed.Value);
    WriteName(writer, snapshot.Metadata.Name);
    WriteWorldSurface(writer, snapshot.Metadata.WorldSurface);
    WriteRockLayer(writer, snapshot.Metadata.RockLayer);
    WriteIsRemixWorld(writer, snapshot.Metadata.IsRemixWorld);
    WriteIsNoTrapsWorld(writer, snapshot.Metadata.IsNoTrapsWorld);
    WriteIsSkyblockWorld(writer, snapshot.Metadata.IsSkyblockWorld);
    WriteIsGoodWorld(writer, snapshot.Metadata.IsGoodWorld);
    for (int y = 0; y < snapshot.Metadata.Height; y++)
    {
      for (int x = 0; x < snapshot.Metadata.Width; x++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        WriteTile(writer, tile);
      }
    }

    int sectionCount = checked((snapshot.Metadata.Width / WorldGrid.SectionWidth) *
      (snapshot.Metadata.Height / WorldGrid.SectionHeight));
    writer.Write(sectionCount);
    for (int y = 0; y < snapshot.Metadata.Height / WorldGrid.SectionHeight; y++)
    {
      for (int x = 0; x < snapshot.Metadata.Width / WorldGrid.SectionWidth; x++)
      {
        writer.Write(snapshot.GetSectionVersion(new WorldSectionCoordinates(x, y)));
      }
    }
  }

  private static string ReadName(BinaryReader reader)
  {
    int byteCount = reader.ReadInt32();
    if (byteCount <= 0 || byteCount > MaximumWorldNameBytes)
    {
      throw new InvalidDataException("The world name length is invalid.");
    }

    byte[] bytes = reader.ReadBytes(byteCount);
    if (bytes.Length != byteCount)
    {
      throw new EndOfStreamException("The world name was truncated.");
    }

    return Encoding.UTF8.GetString(bytes);
  }

  private static WorldTile ReadLegacyTile(BinaryReader reader)
  {
    byte state = reader.ReadByte();
    if (state > 1)
    {
      throw new InvalidDataException("A tile active-state value was not recognized.");
    }

    return new WorldTile(state == 1, reader.ReadUInt16());
  }

  private static double? ReadWorldSurface(BinaryReader reader, int formatVersion)
  {
    if (formatVersion < WorldSurfaceFormatVersion || !reader.ReadBoolean())
    {
      return null;
    }

    return reader.ReadDouble();
  }

  private static bool? ReadIsRemixWorld(BinaryReader reader, int formatVersion)
  {
    if (formatVersion < RemixWorldFormatVersion || !reader.ReadBoolean())
    {
      return null;
    }

    return reader.ReadBoolean();
  }

  private static bool? ReadIsNoTrapsWorld(BinaryReader reader, int formatVersion)
  {
    if (formatVersion < NoTrapsWorldFormatVersion || !reader.ReadBoolean())
    {
      return null;
    }

    return reader.ReadBoolean();
  }

  private static bool? ReadIsSkyblockWorld(BinaryReader reader, int formatVersion)
  {
    if (formatVersion < SkyblockWorldFormatVersion || !reader.ReadBoolean())
    {
      return null;
    }

    return reader.ReadBoolean();
  }

  private static bool? ReadIsGoodWorld(BinaryReader reader, int formatVersion)
  {
    if (formatVersion < GoodWorldFormatVersion || !reader.ReadBoolean())
    {
      return null;
    }

    return reader.ReadBoolean();
  }

  private static double? ReadRockLayer(BinaryReader reader, int formatVersion)
  {
    if (formatVersion < RockLayerFormatVersion || !reader.ReadBoolean())
    {
      return null;
    }

    double rockLayer = reader.ReadDouble();
    return double.IsFinite(rockLayer) ? rockLayer :
      throw new InvalidDataException("The world rock layer is not finite.");
  }


  private static WorldTile ReadTile(BinaryReader reader)
  {
    byte state = reader.ReadByte();
    if (state > 1)
    {
      throw new InvalidDataException("A tile active-state value was not recognized.");
    }

    return new WorldTile(
      IsActive: state == 1,
      Type: reader.ReadUInt16(),
      LiquidAmount: reader.ReadByte(),
      LiquidType: reader.ReadByte(),
      FrameX: reader.ReadInt16(),
      FrameY: reader.ReadInt16(),
      WallType: reader.ReadUInt16(),
      HasWire: reader.ReadBoolean(),
      HasWire2: reader.ReadBoolean(),
      HasWire3: reader.ReadBoolean(),
      HasWire4: reader.ReadBoolean(),
      IsHalfBrick: reader.ReadBoolean(),
      Slope: reader.ReadByte(),
      IsActuated: reader.ReadBoolean(),
      IsInactive: reader.ReadBoolean(),
      TileColor: reader.ReadByte(),
      WallColor: reader.ReadByte(),
      IsInvisibleBlock: reader.ReadBoolean(),
      IsInvisibleWall: reader.ReadBoolean(),
      IsFullbrightBlock: reader.ReadBoolean(),
      IsFullbrightWall: reader.ReadBoolean());
  }

  private static void ValidateDimensions(int width, int height)
  {
    if (width <= 0 || height <= 0 || width % WorldGrid.SectionWidth != 0 ||
        height % WorldGrid.SectionHeight != 0 ||
        (long)width * height > MaximumWorldTileCount)
    {
      throw new InvalidDataException("The world dimensions are not valid section dimensions.");
    }
  }

  private static void WriteName(BinaryWriter writer, string name)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(name);
    if (bytes.Length == 0 || bytes.Length > MaximumWorldNameBytes)
    {
      throw new InvalidDataException("The world name length is invalid.");
    }

    writer.Write(bytes.Length);
    writer.Write(bytes);
  }

  private static void WriteWorldSurface(BinaryWriter writer, double? worldSurface)
  {
    writer.Write(worldSurface.HasValue);
    if (worldSurface.HasValue)
    {
      writer.Write(worldSurface.Value);
    }
  }

  private static void WriteIsRemixWorld(BinaryWriter writer, bool? isRemixWorld)
  {
    writer.Write(isRemixWorld.HasValue);
    if (isRemixWorld.HasValue)
    {
      writer.Write(isRemixWorld.Value);
    }
  }

  private static void WriteIsNoTrapsWorld(BinaryWriter writer, bool? isNoTrapsWorld)
  {
    writer.Write(isNoTrapsWorld.HasValue);
    if (isNoTrapsWorld.HasValue)
    {
      writer.Write(isNoTrapsWorld.Value);
    }
  }

  private static void WriteIsSkyblockWorld(BinaryWriter writer, bool? isSkyblockWorld)
  {
    writer.Write(isSkyblockWorld.HasValue);
    if (isSkyblockWorld.HasValue)
    {
      writer.Write(isSkyblockWorld.Value);
    }
  }

  private static void WriteIsGoodWorld(BinaryWriter writer, bool? isGoodWorld)
  {
    writer.Write(isGoodWorld.HasValue);
    if (isGoodWorld.HasValue)
    {
      writer.Write(isGoodWorld.Value);
    }
  }

  private static void WriteRockLayer(BinaryWriter writer, double? rockLayer)
  {
    writer.Write(rockLayer.HasValue);
    if (rockLayer.HasValue)
    {
      writer.Write(rockLayer.Value);
    }
  }


  private static void WriteTile(BinaryWriter writer, WorldTile tile)
  {
    writer.Write(tile.IsActive ? (byte)1 : (byte)0);
    writer.Write(tile.Type);
    writer.Write(tile.LiquidAmount);
    writer.Write(tile.LiquidType);
    writer.Write(tile.FrameX);
    writer.Write(tile.FrameY);
    writer.Write(tile.WallType);
    writer.Write(tile.HasWire);
    writer.Write(tile.HasWire2);
    writer.Write(tile.HasWire3);
    writer.Write(tile.HasWire4);
    writer.Write(tile.IsHalfBrick);
    writer.Write(tile.Slope);
    writer.Write(tile.IsActuated);
    writer.Write(tile.IsInactive);
    writer.Write(tile.TileColor);
    writer.Write(tile.WallColor);
    writer.Write(tile.IsInvisibleBlock);
    writer.Write(tile.IsInvisibleWall);
    writer.Write(tile.IsFullbrightBlock);
    writer.Write(tile.IsFullbrightWall);
  }
}
