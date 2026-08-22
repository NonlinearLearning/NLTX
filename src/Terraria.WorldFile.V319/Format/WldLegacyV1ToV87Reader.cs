using System;
using System.Collections.Generic;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

internal static class WldLegacyV1ToV87Reader
{
  private const int ChestRecordCount = 1_000;
  private const int SignRecordCount = 1_000;

  private static readonly HashSet<ushort> FrameImportantTileTypes =
  [
    3, 4, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 26, 27, 28,
    29, 31, 33, 34, 35, 36, 42, 49, 50, 55, 61, 71, 72, 73, 74, 77, 78, 79,
    81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98,
    99, 100, 101, 102, 103, 104, 105, 106, 110, 113, 114, 125, 126, 128,
    129, 132, 133, 134, 135, 136, 137, 138, 139, 141, 142, 143, 144, 149,
    165, 171, 172, 173, 174, 178, 184, 185, 186, 187, 201, 207, 209, 210,
    212, 215, 216, 217, 218, 219, 220, 227, 228, 231, 233, 235, 236, 237,
    238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 254, 269, 270, 271,
    275, 276, 277, 278, 279, 280, 281, 282, 283, 285, 286, 287, 288, 289,
    290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300
  ];

  public static LegacyWorldDocument Read(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits)
  {
    LegacyWorldMetadata metadata = ReadHeader(version, reader, limits);
    List<LegacyTile> tiles = ReadTiles(version, reader, metadata, limits);
    List<LegacyChest> chests = ReadChests(version, reader, metadata, limits);
    List<LegacySign> signs = ReadSigns(reader, metadata, limits);
    List<LegacyNpc> npcs = ReadNpcs(version, reader, limits);
    ReadNpcNames(version, reader);
    VerifyFooter(version, reader, metadata);
    reader.RequireSectionEnd();

    return new LegacyWorldDocument(
      version,
      WldFormatVersion.LegacyV1ToV87,
      metadata,
      tiles,
      chests,
      signs,
      npcs,
      Array.Empty<LegacyTileEntity>(),
      Array.Empty<LegacySectionDiagnostic>());
  }

  private static List<LegacyChest> ReadChests(
    int version,
    WldBinaryReader reader,
    LegacyWorldMetadata metadata,
    WldReadLimits limits)
  {
    int slotCount = version < 58 ? 20 : 40;
    List<LegacyChest> chests = [];
    for (int index = 0; index < ChestRecordCount; index++)
    {
      if (!reader.ReadBoolean())
      {
        continue;
      }

      int x = reader.ReadInt32();
      int y = reader.ReadInt32();
      ValidateCoordinates(x, y, metadata, "chest");
      if (chests.Count >= limits.MaxEntityCount)
      {
        throw new InvalidDataException("The WLD chest count exceeds configured limits.");
      }

      string name = version >= 85 ? reader.ReadString() : string.Empty;
      List<LegacyChestItem> items = new(slotCount);
      for (int slot = 0; slot < slotCount; slot++)
      {
        short stack = version < 59 ? reader.ReadByte() : reader.ReadInt16();
        int netId = 0;
        byte prefix = 0;
        if (stack > 0)
        {
          if (version >= 38)
          {
            netId = reader.ReadInt32();
          }
          else
          {
            _ = reader.ReadString();
          }

          if (version >= 36)
          {
            prefix = reader.ReadByte();
          }
        }

        items.Add(new LegacyChestItem(stack, netId, prefix));
      }

      chests.Add(new LegacyChest(x, y, name, items));
    }

    return chests;
  }

  private static LegacyWorldMetadata ReadHeader(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits)
  {
    string name = reader.ReadString();
    int worldId = reader.ReadInt32();
    int leftWorld = reader.ReadInt32();
    int rightWorld = reader.ReadInt32();
    int topWorld = reader.ReadInt32();
    int bottomWorld = reader.ReadInt32();
    int height = reader.ReadInt32();
    int width = reader.ReadInt32();
    limits.ValidateWorldDimensions(width, height);
    if (version >= 63)
    {
      _ = reader.ReadByte();
    }

    if (version >= 44)
    {
      ReadInt32Values(reader, 7);
    }

    if (version >= 60)
    {
      ReadInt32Values(reader, version >= 61 ? 10 : 8);
    }

    int spawnX = reader.ReadInt32();
    int spawnY = reader.ReadInt32();
    ValidateCoordinates(spawnX, spawnY, width, height, "spawn");
    double worldSurface = reader.ReadDouble();
    double rockLayer = reader.ReadDouble();
    double timeOfDay = reader.ReadDouble();
    bool isDayTime = reader.ReadBoolean();
    byte moonPhase = ReadMoonPhase(reader);
    bool isBloodMoon = reader.ReadBoolean();
    bool isEclipse = false;
    if (version >= 70)
    {
      isEclipse = reader.ReadBoolean();
    }

    _ = reader.ReadInt32();
    _ = reader.ReadInt32();
    bool isCrimsonWorld = false;
    if (version >= 56)
    {
      isCrimsonWorld = reader.ReadBoolean();
    }

    bool defeatedEyeOfCthulhu = reader.ReadBoolean();
    bool defeatedEaterOrBrain = reader.ReadBoolean();
    bool defeatedSkeletron = reader.ReadBoolean();
    if (version >= 66)
    {
      _ = reader.ReadBoolean();
    }

    bool defeatedMechanicalBoss = false;
    if (version >= 44)
    {
      ReadBooleanValues(reader, 3);
      defeatedMechanicalBoss = reader.ReadBoolean();
    }

    bool defeatedPlantera = false;
    bool defeatedGolem = false;
    bool defeatedGoblins = false;
    bool defeatedFrost = false;
    bool defeatedPirates = false;
    if (version >= 64)
    {
      defeatedPlantera = reader.ReadBoolean();
      defeatedGolem = reader.ReadBoolean();
    }

    if (version >= 29)
    {
      ReadBooleanValues(reader, 2);
      if (version >= 34)
      {
        _ = reader.ReadBoolean();
        if (version >= 80)
        {
          _ = reader.ReadBoolean();
        }
      }

      defeatedGoblins = reader.ReadBoolean();
    }

    if (version >= 32)
    {
      _ = reader.ReadBoolean();
    }

    if (version >= 37)
    {
      defeatedFrost = reader.ReadBoolean();
    }

    if (version >= 56)
    {
      defeatedPirates = reader.ReadBoolean();
    }

    _ = reader.ReadBoolean();
    bool isMeteorScheduled = reader.ReadBoolean();
    _ = reader.ReadByte();
    bool isHardMode = false;
    if (version >= 23)
    {
      _ = reader.ReadInt32();
      isHardMode = reader.ReadBoolean();
    }

    _ = reader.ReadInt32();
    int invasionSize = reader.ReadInt32();
    int invasionType = reader.ReadInt32();
    double invasionX = reader.ReadDouble();
    bool? isRaining = null;
    int? rainTimeTicks = null;
    float? maximumRainStrength = null;
    if (version >= 53)
    {
      isRaining = reader.ReadBoolean();
      rainTimeTicks = reader.ReadInt32();
      maximumRainStrength = reader.ReadSingle();
    }

    if (version >= 54)
    {
      ReadInt32Values(reader, 3);
    }

    if (version >= 55)
    {
      ReadByteValues(reader, 3);
    }

    if (version >= 60)
    {
      ReadByteValues(reader, 5);
      _ = reader.ReadInt32();
    }

    float? windSpeedTarget = null;
    if (version >= 62)
    {
      _ = reader.ReadInt16();
      windSpeedTarget = reader.ReadSingle();
    }

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
      GameMode: 0,
      IsMeteorScheduled: isMeteorScheduled,
      WindSpeedTarget: windSpeedTarget,
      IsRaining: isRaining,
      RainTimeTicks: rainTimeTicks,
      MaximumRainStrength: maximumRainStrength,
      IsRemixWorld: null,
      TimeOfDay: timeOfDay,
      IsDayTime: isDayTime);
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

  private static List<LegacyNpc> ReadNpcs(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits)
  {
    List<LegacyNpc> npcs = [];
    while (reader.ReadBoolean())
    {
      if (npcs.Count >= limits.MaxEntityCount)
      {
        throw new InvalidDataException("The WLD NPC count exceeds configured limits.");
      }

      string typeName = reader.ReadString();
      string name = version >= 83 ? reader.ReadString() : typeName;
      float positionX = reader.ReadSingle();
      float positionY = reader.ReadSingle();
      bool isHomeless = reader.ReadBoolean();
      int homeX = reader.ReadInt32();
      int homeY = reader.ReadInt32();
      npcs.Add(new LegacyNpc(0, name, positionX, positionY, isHomeless, homeX, homeY));
    }

    return npcs;
  }

  private static void ReadNpcNames(int version, WldBinaryReader reader)
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
      _ = reader.ReadString();
    }
  }

  private static List<LegacySign> ReadSigns(
    WldBinaryReader reader,
    LegacyWorldMetadata metadata,
    WldReadLimits limits)
  {
    List<LegacySign> signs = [];
    for (int index = 0; index < SignRecordCount; index++)
    {
      if (!reader.ReadBoolean())
      {
        continue;
      }

      string text = reader.ReadString();
      int x = reader.ReadInt32();
      int y = reader.ReadInt32();
      ValidateCoordinates(x, y, metadata, "sign");
      if (signs.Count >= limits.MaxEntityCount)
      {
        throw new InvalidDataException("The WLD sign count exceeds configured limits.");
      }

      signs.Add(new LegacySign(x, y, text));
    }

    return signs;
  }

  private static List<LegacyTile> ReadTiles(
    int version,
    WldBinaryReader reader,
    LegacyWorldMetadata metadata,
    WldReadLimits limits)
  {
    int tileCount = checked(metadata.Width * metadata.Height);
    List<LegacyTile> tiles = new(tileCount);
    for (int x = 0; x < metadata.Width; x++)
    {
      for (int y = 0; y < metadata.Height; y++)
      {
        LegacyTile tile = ReadTile(version, reader);
        int runLength = version >= 25 ? reader.ReadInt16() : 0;
        if (runLength < 0 || runLength > metadata.Height - y - 1)
        {
          throw new InvalidDataException("The WLD tile RLE run crosses a row boundary.");
        }

        for (int repeat = 0; repeat <= runLength; repeat++)
        {
          if (tiles.Count >= limits.MaxTileCount)
          {
            throw new InvalidDataException("The WLD tile count exceeds configured limits.");
          }

          tiles.Add(tile);
        }

        y += runLength;
      }
    }

    return tiles;
  }

  private static LegacyTile ReadTile(int version, WldBinaryReader reader)
  {
    bool isActive = reader.ReadBoolean();
    ushort tileType = 0;
    short frameX = -1;
    short frameY = -1;
    byte tileColor = 0;
    if (isActive)
    {
      tileType = version <= 77 ? reader.ReadByte() : reader.ReadUInt16();
      if (RequiresFrame(version, tileType))
      {
        frameX = reader.ReadInt16();
        frameY = reader.ReadInt16();
        if (tileType == 144)
        {
          frameY = 0;
        }
      }

      if (version >= 48 && reader.ReadBoolean())
      {
        tileColor = reader.ReadByte();
      }
    }

    if (version <= 25)
    {
      _ = reader.ReadBoolean();
    }

    ushort wallType = 0;
    byte wallColor = 0;
    if (reader.ReadBoolean())
    {
      wallType = reader.ReadByte();
      if (version >= 48 && reader.ReadBoolean())
      {
        wallColor = reader.ReadByte();
      }
    }

    byte liquidAmount = 0;
    byte liquidKind = 0;
    if (reader.ReadBoolean())
    {
      liquidAmount = reader.ReadByte();
      liquidKind = reader.ReadBoolean() ? (byte)1 : (byte)0;
      if (version >= 51 && reader.ReadBoolean())
      {
        liquidKind = 2;
      }
    }

    bool hasWire = version >= 33 && reader.ReadBoolean();
    bool hasWire2 = false;
    bool hasWire3 = false;
    if (version >= 43)
    {
      hasWire2 = reader.ReadBoolean();
      hasWire3 = reader.ReadBoolean();
    }

    bool isHalfBrick = false;
    byte slope = 0;
    if (version >= 41)
    {
      isHalfBrick = reader.ReadBoolean();
      if (version >= 49)
      {
        slope = reader.ReadByte();
      }
    }

    bool isActuated = false;
    bool isInactive = false;
    if (version >= 42)
    {
      isActuated = reader.ReadBoolean();
      isInactive = reader.ReadBoolean();
    }

    return new LegacyTile(
      isActive,
      tileType,
      wallType,
      liquidAmount,
      liquidKind,
      hasWire,
      hasWire2,
      hasWire3,
      false,
      isHalfBrick,
      slope,
      isActuated,
      isInactive,
      frameX,
      frameY,
      tileColor,
      wallColor);
  }

  private static void ReadBooleanValues(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadBoolean();
    }
  }

  private static void ReadByteValues(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadByte();
    }
  }

  private static void ReadInt32Values(WldBinaryReader reader, int count)
  {
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadInt32();
    }
  }

  private static bool RequiresFrame(int version, ushort tileType)
  {
    if (version < 72 && tileType is 35 or 36 or 170 or 171 or 172)
    {
      return true;
    }

    if (version < 28 && tileType == 4)
    {
      return false;
    }

    if (version < 40 && tileType == 19)
    {
      return false;
    }

    if (version < 195 && tileType == 49)
    {
      return false;
    }

    return FrameImportantTileTypes.Contains(tileType);
  }

  private static void ValidateCoordinates(
    int x,
    int y,
    LegacyWorldMetadata metadata,
    string recordName)
  {
    ValidateCoordinates(x, y, metadata.Width, metadata.Height, recordName);
  }

  private static void ValidateCoordinates(
    int x,
    int y,
    int width,
    int height,
    string recordName)
  {
    if (x < 0 || x >= width || y < 0 || y >= height)
    {
      throw new InvalidDataException($"The WLD {recordName} coordinates are outside the world.");
    }
  }

  private static void VerifyFooter(
    int version,
    WldBinaryReader reader,
    LegacyWorldMetadata metadata)
  {
    if (version < 7)
    {
      return;
    }

    bool isValid = reader.ReadBoolean();
    string worldName = reader.ReadString();
    int worldId = reader.ReadInt32();
    if (!isValid || !StringComparer.Ordinal.Equals(worldName, metadata.Name) ||
        worldId != metadata.WorldId)
    {
      throw new InvalidDataException("The WLD footer identity does not match the header.");
    }
  }
}
