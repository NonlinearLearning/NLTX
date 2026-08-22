using System;
using System.IO;

namespace Terraria.WorldFile.V319.Verification.Fixtures;

internal static class LegacyV1ToV87FixtureWriter
{
  private const int Height = 2;
  private const int Width = 2;
  private const int WorldId = 2468;
  private const string WorldName = "Legacy fixture";

  public static MemoryStream Create(int version)
  {
    return Create(
      version,
      rleRunLength: 1,
      isBloodMoon: false,
      isEclipse: false,
      isCrimsonWorld: false,
      isHardMode: false,
      invasionType: 0,
      invasionSize: 0,
      isMeteorScheduled: false);
  }

  public static MemoryStream Create(
    int version,
    bool isBloodMoon,
    bool isEclipse,
    bool isCrimsonWorld = false,
    bool isHardMode = false,
    int invasionType = 0,
    int invasionSize = 0,
    bool isMeteorScheduled = false)
  {
    return Create(
      version,
      rleRunLength: 1,
      isBloodMoon,
      isEclipse,
      isCrimsonWorld,
      isHardMode,
      invasionType,
      invasionSize,
      isMeteorScheduled);
  }

  public static MemoryStream CreateRleCrossingRowBoundary()
  {
    return Create(
      version: 87,
      rleRunLength: Height,
      isBloodMoon: false,
      isEclipse: false,
      isCrimsonWorld: false,
      isHardMode: false,
      invasionType: 0,
      invasionSize: 0,
      isMeteorScheduled: false);
  }

  private static MemoryStream Create(
    int version,
    short rleRunLength,
    bool isBloodMoon,
    bool isEclipse,
    bool isCrimsonWorld,
    bool isHardMode,
    int invasionType,
    int invasionSize,
    bool isMeteorScheduled)
  {
    if (version < 1 || version > 87)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    MemoryStream stream = new();
    using (BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      WriteHeader(
        writer,
        version,
        isBloodMoon,
        isEclipse,
        isCrimsonWorld,
        isHardMode,
        invasionType,
        invasionSize,
        isMeteorScheduled);
      WriteTiles(writer, version, rleRunLength);
      WriteChests(writer, version);
      WriteSigns(writer);
      writer.Write(false);
      WriteNpcNames(writer, version);
      if (version >= 7)
      {
        writer.Write(true);
        writer.Write(WorldName);
        writer.Write(WorldId);
      }
    }

    stream.Position = 0;
    return stream;
  }

  private static void WriteChests(BinaryWriter writer, int version)
  {
    writer.Write(true);
    writer.Write(0);
    writer.Write(0);
    if (version >= 85)
    {
      writer.Write("Legacy chest");
    }

    int slotCount = version < 58 ? 20 : 40;
    for (int index = 0; index < slotCount; index++)
    {
      if (version < 59)
      {
        writer.Write((byte)0);
      }
      else
      {
        writer.Write((short)0);
      }
    }

    for (int index = 1; index < 1000; index++)
    {
      writer.Write(false);
    }
  }

  private static void WriteHeader(
    BinaryWriter writer,
    int version,
    bool isBloodMoon,
    bool isEclipse,
    bool isCrimsonWorld,
    bool isHardMode,
    int invasionType,
    int invasionSize,
    bool isMeteorScheduled)
  {
    writer.Write(version);
    writer.Write(WorldName);
    writer.Write(WorldId);
    writer.Write(-84);
    writer.Write(84);
    writer.Write(-60);
    writer.Write(60);
    writer.Write(Height);
    writer.Write(Width);
    if (version >= 63)
    {
      writer.Write((byte)0);
    }

    if (version >= 44)
    {
      for (int index = 0; index < 7; index++)
      {
        writer.Write(0);
      }
    }

    if (version >= 60)
    {
      int caveBackgroundCount = version >= 61 ? 10 : 8;
      for (int index = 0; index < caveBackgroundCount; index++)
      {
        writer.Write(0);
      }
    }

    writer.Write(1);
    writer.Write(1);
    writer.Write(0.0d);
    writer.Write(1.0d);
    writer.Write(2.0d);
    writer.Write(true);
    writer.Write(0);
    writer.Write(isBloodMoon);
    if (version >= 70)
    {
      writer.Write(isEclipse);
    }

    writer.Write(0);
    writer.Write(0);
    if (version >= 56)
    {
      writer.Write(isCrimsonWorld);
    }

    for (int index = 0; index < 3; index++)
    {
      writer.Write(false);
    }

    if (version >= 66)
    {
      writer.Write(false);
    }

    if (version >= 44)
    {
      for (int index = 0; index < 4; index++)
      {
        writer.Write(false);
      }
    }

    if (version >= 64)
    {
      writer.Write(false);
      writer.Write(false);
    }

    if (version >= 29)
    {
      writer.Write(false);
      writer.Write(false);
      if (version >= 34)
      {
        writer.Write(false);
        if (version >= 80)
        {
          writer.Write(false);
        }
      }

      writer.Write(false);
    }

    if (version >= 32)
    {
      writer.Write(false);
    }

    if (version >= 37)
    {
      writer.Write(false);
    }

    if (version >= 56)
    {
      writer.Write(false);
    }

    writer.Write(false);
    writer.Write(isMeteorScheduled);
    writer.Write((byte)0);
    if (version >= 23)
    {
      writer.Write(0);
      writer.Write(isHardMode);
    }

    writer.Write(0);
    writer.Write(invasionSize);
    writer.Write(invasionType);
    writer.Write(0.0d);
    if (version >= 53)
    {
      writer.Write(false);
      writer.Write(0);
      writer.Write(0.0f);
    }

    if (version >= 54)
    {
      for (int index = 0; index < 3; index++)
      {
        writer.Write(0);
      }
    }

    if (version >= 55)
    {
      for (int index = 0; index < 3; index++)
      {
        writer.Write((byte)0);
      }
    }

    if (version >= 60)
    {
      for (int index = 0; index < 5; index++)
      {
        writer.Write((byte)0);
      }

      writer.Write(0);
    }

    if (version >= 62)
    {
      writer.Write((short)0);
      writer.Write(0.0f);
    }
  }

  private static void WriteSigns(BinaryWriter writer)
  {
    writer.Write(true);
    writer.Write("Legacy sign");
    writer.Write(0);
    writer.Write(0);
    for (int index = 1; index < 1000; index++)
    {
      writer.Write(false);
    }
  }

  private static void WriteNpcNames(BinaryWriter writer, int version)
  {
    if (version < 31 || version > 83)
    {
      return;
    }

    int nameCount = 9;
    if (version >= 35)
    {
      nameCount++;
    }

    if (version >= 65)
    {
      nameCount += 8;
    }

    if (version >= 79)
    {
      nameCount++;
    }

    for (int index = 0; index < nameCount; index++)
    {
      writer.Write(string.Empty);
    }
  }

  private static void WriteTiles(BinaryWriter writer, int version, short rleRunLength)
  {
    for (int x = 0; x < Width; x++)
    {
      for (int y = 0; y < Height; y++)
      {
        short runLength = version >= 25 && y == 0 ? rleRunLength : (short)0;
        WriteV87Tile(writer, version, runLength);
        if (runLength > 0)
        {
          y += runLength;
        }
      }
    }
  }

  private static void WriteV87Tile(BinaryWriter writer, int version, short rleRunLength)
  {
    writer.Write(false);
    if (version <= 25)
    {
      writer.Write(false);
    }

    writer.Write(false);
    writer.Write(false);
    if (version >= 33)
    {
      writer.Write(false);
    }

    if (version >= 43)
    {
      writer.Write(false);
      writer.Write(false);
    }

    if (version >= 41)
    {
      writer.Write(false);
      if (version >= 49)
      {
        writer.Write((byte)0);
      }
    }

    if (version >= 42)
    {
      writer.Write(false);
      writer.Write(false);
    }

    if (version >= 25)
    {
      writer.Write(rleRunLength);
    }
  }
}
