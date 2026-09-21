using System;
using System.IO;
using System.Globalization;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

internal static class WldHeaderReader
{
  public static LegacyWorldMetadata Read(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits)
  {
    string name = reader.ReadString();
    ulong? worldGeneratorVersion = null;
    Guid? uniqueId = null;
    string? seedText = null;
    if (version >= 179)
    {
      if (version == 179)
      {
        seedText = reader.ReadInt32().ToString(CultureInfo.InvariantCulture);
      }
      else
      {
        seedText = reader.ReadString();
      }

      worldGeneratorVersion = reader.ReadUInt64();
    }

    if (version >= 181)
    {
      uniqueId = new Guid(reader.ReadBytes(16));
    }

    int worldId = reader.ReadInt32();
    int leftWorld = reader.ReadInt32();
    int rightWorld = reader.ReadInt32();
    int topWorld = reader.ReadInt32();
    int bottomWorld = reader.ReadInt32();
    int height = reader.ReadInt32();
    int width = reader.ReadInt32();
    limits.ValidateWorldDimensions(width, height);

    int gameMode = 0;
    bool? isRemixWorld = null;
    bool? isNoTrapsWorld = null;
    bool? isSkyblockWorld = null;
    bool? isGoodWorld = null;
    if (version >= 209)
    {
      gameMode = reader.ReadInt32();
      if (version >= 222)
      {
        isRemixWorld = ReadWorldVariantBooleans(
          version,
          reader,
          out isGoodWorld,
          out isNoTrapsWorld,
          out isSkyblockWorld);
      }
    }
    else if (version >= 112)
    {
      gameMode = reader.ReadBoolean() ? 1 : 0;
      if (version == 208 && reader.ReadBoolean())
      {
        gameMode = 2;
      }
    }

    if (version >= 141)
    {
      _ = reader.ReadInt64();
    }

    if (version >= 284)
    {
      _ = reader.ReadInt64();
    }

    _ = reader.ReadByte();
    ReadInt32Values(reader, 17);
    int spawnX = NormalizeCoordinate(reader.ReadInt32(), width);
    int spawnY = NormalizeCoordinate(reader.ReadInt32(), height);
    ValidateCoordinates(spawnX, spawnY, width, height);
    double worldSurface = reader.ReadDouble();
    double rockLayer = reader.ReadDouble();
    double timeOfDay = reader.ReadDouble();
    bool isDayTime = reader.ReadBoolean();
    byte moonPhase = ReadMoonPhase(reader);
    bool isBloodMoon = reader.ReadBoolean();
    bool isEclipse = reader.ReadBoolean();
    ReadInt32Values(reader, 2);
    bool isCrimsonWorld = reader.ReadBoolean();
    bool defeatedEyeOfCthulhu = reader.ReadBoolean();
    bool defeatedEaterOrBrain = reader.ReadBoolean();
    bool defeatedSkeletron = reader.ReadBoolean();
    _ = reader.ReadBoolean();
    ReadBooleanValues(reader, 3);
    bool defeatedMechanicalBoss = reader.ReadBoolean();
    bool defeatedPlantera = reader.ReadBoolean();
    bool defeatedGolem = reader.ReadBoolean();
    if (version >= 118)
    {
      _ = reader.ReadBoolean();
    }

    bool defeatedGoblins = false;
    bool defeatedFrost = false;
    bool defeatedPirates = false;
    for (int index = 0; index < 7; index++)
    {
      bool value = reader.ReadBoolean();
      if (index == 3)
      {
        defeatedGoblins = value;
      }
      else if (index == 5)
      {
        defeatedFrost = value;
      }
      else if (index == 6)
      {
        defeatedPirates = value;
      }
    }
    _ = reader.ReadBoolean();
    bool isMeteorScheduled = reader.ReadBoolean();
    _ = reader.ReadByte();
    _ = reader.ReadInt32();
    bool isHardMode = reader.ReadBoolean();
    if (version >= 257)
    {
      _ = reader.ReadBoolean();
    }

    _ = reader.ReadInt32();
    int invasionSize = reader.ReadInt32();
    int invasionType = reader.ReadInt32();
    double invasionX = reader.ReadDouble();
    if (version >= 118)
    {
      _ = reader.ReadDouble();
    }

    if (version >= 113)
    {
      _ = reader.ReadByte();
    }

    bool? isRaining = null;
    int? rainTimeTicks = null;
    float? maximumRainStrength = null;
    if (version >= 53)
    {
      isRaining = reader.ReadBoolean();
      rainTimeTicks = reader.ReadInt32();
      maximumRainStrength = reader.ReadSingle();
    }
    ReadInt32Values(reader, 3);
    ReadByteValues(reader, 8);
    _ = reader.ReadInt32();
    float? windSpeedTarget = null;
    if (version >= 62)
    {
      _ = reader.ReadInt16();
      windSpeedTarget = reader.ReadSingle();
    }
    ReadLaterFields(version, reader, limits, out bool defeatedMartians);

    return new LegacyWorldMetadata(
      name,
      worldId,
      width,
      height,
      spawnX,
      spawnY,
      leftWorld,
      rightWorld,
      topWorld,
      bottomWorld,
      worldSurface,
      rockLayer,
      moonPhase,
      isBloodMoon,
      isEclipse,
      isCrimsonWorld,
      isHardMode,
      defeatedEyeOfCthulhu,
      defeatedEaterOrBrain,
      defeatedSkeletron,
      defeatedMechanicalBoss,
      defeatedPlantera,
      defeatedGolem,
      InvasionType: invasionType,
      InvasionSize: invasionSize,
      InvasionX: invasionX,
      DefeatedGoblins: defeatedGoblins,
      DefeatedFrost: defeatedFrost,
      DefeatedPirates: defeatedPirates,
      DefeatedMartians: defeatedMartians,
      GameMode: gameMode,
      IsMeteorScheduled: isMeteorScheduled,
      WindSpeedTarget: windSpeedTarget,
      IsRaining: isRaining,
      RainTimeTicks: rainTimeTicks,
      MaximumRainStrength: maximumRainStrength,
      IsRemixWorld: isRemixWorld,
      TimeOfDay: timeOfDay,
      IsDayTime: isDayTime,
      WorldGeneratorVersion: worldGeneratorVersion,
      UniqueId: uniqueId,
      SeedText: seedText,
      IsNoTrapsWorld: isNoTrapsWorld,
      IsSkyblockWorld: isSkyblockWorld,
      IsGoodWorld: isGoodWorld);
  }

  private static bool? ReadWorldVariantBooleans(
    int version,
    WldBinaryReader reader,
    out bool? isGoodWorld,
    out bool? isNoTrapsWorld,
    out bool? isSkyblockWorld)
  {
    bool? isRemixWorld = null;
    isGoodWorld = null;
    isNoTrapsWorld = null;
    isSkyblockWorld = null;
    int count = version >= 302 ? 9 : 8;
    for (int index = 0; index < count; index++)
    {
      bool value = reader.ReadBoolean();
      if (version >= 249 && index == 5)
      {
        isRemixWorld = value;
      }

      if (version >= 227 && index == 1)
      {
        isGoodWorld = value;
      }

      if (version >= 266 && index == 6)
      {
        isNoTrapsWorld = value;
      }

      if (version >= 302 && index == 8)
      {
        isSkyblockWorld = value;
      }
    }

    return isRemixWorld;
  }

  private static void ReadLaterFields(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits,
    out bool defeatedMartians)
  {
    defeatedMartians = false;
    if (version < 95)
    {
      return;
    }

    ReadStringValues(reader, ReadCount(reader, limits, "angler"));
    if (version < 99)
    {
      return;
    }

    _ = reader.ReadBoolean();
    if (version < 101)
    {
      return;
    }

    _ = reader.ReadInt32();
    if (version < 104)
    {
      return;
    }

    _ = reader.ReadBoolean();
    if (version >= 129)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 201)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 107)
    {
      _ = reader.ReadInt32();
    }

    if (version >= 108)
    {
      _ = reader.ReadInt32();
    }

    if (version < 109)
    {
      return;
    }

    ReadInt32Values(reader, ReadInt16Count(reader, limits, "banner"));
    if (version >= 289)
    {
      ReadUInt16Values(reader, ReadInt16Count(reader, limits, "claimable banner"));
    }

    if (version < 128)
    {
      return;
    }

    _ = reader.ReadBoolean();
    if (version < 131)
    {
      return;
    }

    for (int index = 0; index < 9; index++)
    {
      bool value = reader.ReadBoolean();
      if (index == 1)
      {
        defeatedMartians = value;
      }
    }
    if (version < 140)
    {
      return;
    }

    ReadBooleanValues(reader, 9);
    if (version >= 170)
    {
      ReadBooleanValues(reader, 2);
      _ = reader.ReadInt32();
      ReadInt32Values(reader, ReadCount(reader, limits, "party NPC"));
    }

    if (version >= 174)
    {
      _ = reader.ReadBoolean();
      _ = reader.ReadInt32();
      _ = reader.ReadSingle();
      _ = reader.ReadSingle();
    }

    if (version >= 178)
    {
      ReadBooleanValues(reader, 4);
    }

    if (version > 194)
    {
      _ = reader.ReadByte();
    }

    if (version >= 215)
    {
      _ = reader.ReadByte();
    }

    if (version > 195)
    {
      ReadByteValues(reader, 3);
    }

    if (version >= 204)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 207)
    {
      _ = reader.ReadInt32();
      ReadBooleanValues(reader, 3);
    }

    if (version >= 211)
    {
      ReadInt32Values(reader, ReadCount(reader, limits, "tree-top"));
    }

    if (version >= 212)
    {
      ReadBooleanValues(reader, 2);
    }

    if (version >= 216)
    {
      ReadInt32Values(reader, 4);
    }

    if (version >= 217)
    {
      ReadBooleanValues(reader, 3);
    }

    if (version >= 223)
    {
      ReadBooleanValues(reader, 2);
    }

    if (version >= 240)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 250)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 251)
    {
      ReadBooleanValues(reader, 8);
    }

    if (version >= 259)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 260)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 261)
    {
      ReadBooleanValues(reader, 7);
    }

    if (version >= 264)
    {
      _ = reader.ReadBoolean();
      _ = reader.ReadByte();
    }

    if (version >= 287)
    {
      ReadBooleanValues(reader, 2);
    }

    if (version >= 288)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 296)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 291)
    {
      ReadInt32Values(reader, 2);
    }

    if (version >= 297)
    {
      _ = reader.ReadBoolean();
      int spawnPointCount = reader.ReadByte();
      for (int index = 0; index < spawnPointCount; index++)
      {
        _ = reader.ReadInt16();
        _ = reader.ReadInt16();
      }
    }

    if (version >= 304)
    {
      _ = reader.ReadBoolean();
    }

    if (version is >= 299 and < 313)
    {
      _ = reader.ReadUInt32();
    }

    if (version >= 299)
    {
      _ = reader.ReadString();
    }
  }

  private static void ReadBooleanValues(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadBoolean();
    }
  }

  private static byte ReadMoonPhase(WldBinaryReader reader)
  {
    int moonPhase = reader.ReadInt32();
    if (moonPhase is < 0 or > 7)
    {
      throw new InvalidDataException("The WLD moon phase is outside the supported range.");
    }

    return (byte)moonPhase;
  }

  private static void ReadByteValues(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadByte();
    }
  }

  private static void ReadDoubleValues(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadDouble();
    }
  }

  private static void ReadInt32Values(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadInt32();
    }
  }

  private static int ReadCount(WldBinaryReader reader, WldReadLimits limits, string recordName)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException($"The WLD {recordName} count exceeds configured limits.");
    }

    return count;
  }

  private static int ReadInt16Count(
    WldBinaryReader reader,
    WldReadLimits limits,
    string recordName)
  {
    short count = reader.ReadInt16();
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException($"The WLD {recordName} count exceeds configured limits.");
    }

    return count;
  }

  private static void ReadStringValues(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadString();
    }
  }

  private static void ReadUInt16Values(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadUInt16();
    }
  }

  private static void ValidateCoordinates(int x, int y, int width, int height)
  {
    if (x < 0 || x >= width || y < 0 || y >= height)
    {
      throw new InvalidDataException("The WLD spawn coordinates are outside the world.");
    }
  }

  private static int NormalizeCoordinate(int value, int limit)
  {
    if (value >= 0 && value < limit)
    {
      return value;
    }

    if (value >= 0 && value % 256 == 0 && value / 256 < limit)
    {
      return value / 256;
    }

    return value;
  }
}
