using System;
using System.Collections.Generic;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldNpcReader
{
  public static IReadOnlyList<LegacyNpc> Read(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits)
  {
    const string sectionName = "npcs";
    int recordIndex = -1;
    try
    {
      if (version >= 268)
      {
        int shimmeredNpcCount = reader.ReadInt32();
        ValidateCount(shimmeredNpcCount, limits, "shimmered NPC");
        for (int index = 0; index < shimmeredNpcCount; index++)
        {
          _ = reader.ReadInt32();
        }
      }

      List<LegacyNpc> npcs = [];
      ReadTownNpcs(version, reader, limits, npcs, ref recordIndex);
      if (version >= 140)
      {
        ReadNonTownNpcs(version, reader, limits, npcs, ref recordIndex);
      }

      reader.RequireSectionEnd();
      return npcs;
    }
    catch (InvalidDataException exception)
    {
      throw new InvalidDataException(
        $"WLD read failed: version={version}, section={sectionName}, record={recordIndex}, " +
        $"offset={reader.Position}. {exception.Message}",
        exception);
    }
  }

  private static void ReadNonTownNpcs(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits,
    List<LegacyNpc> npcs,
    ref int recordIndex)
  {
    while (reader.ReadBoolean())
    {
      EnsureCapacity(npcs, limits);
      recordIndex = npcs.Count;
      (int type, string legacyTypeName) = ReadType(version, reader);
      float positionX = reader.ReadSingle();
      float positionY = reader.ReadSingle();
      npcs.Add(new LegacyNpc(
        type,
        string.Empty,
        positionX,
        positionY,
        false,
        0,
        0,
        legacyTypeName,
        IsTownNpc: false));
    }
  }

  private static void ReadTownNpcs(
    int version,
    WldBinaryReader reader,
    WldReadLimits limits,
    List<LegacyNpc> npcs,
    ref int recordIndex)
  {
    while (reader.ReadBoolean())
    {
      EnsureCapacity(npcs, limits);
      recordIndex = npcs.Count;
      (int type, string legacyTypeName) = ReadType(version, reader);
      string name = reader.ReadString();
      float positionX = reader.ReadSingle();
      float positionY = reader.ReadSingle();
      bool isHomeless = reader.ReadBoolean();
      int homeX = reader.ReadInt32();
      int homeY = reader.ReadInt32();
      int townVariationIndex = 0;
      if (version >= 213 && (reader.ReadByte() & 1) != 0)
      {
        townVariationIndex = reader.ReadInt32();
      }

      bool homelessDespawn = version >= 315 && reader.ReadBoolean();
      npcs.Add(new LegacyNpc(
        type,
        name,
        positionX,
        positionY,
        isHomeless,
        homeX,
        homeY,
        legacyTypeName,
        IsTownNpc: true,
        townVariationIndex,
        homelessDespawn));
    }
  }

  private static (int Type, string LegacyTypeName) ReadType(int version, WldBinaryReader reader)
  {
    return version >= 190
      ? (reader.ReadInt32(), string.Empty)
      : (0, reader.ReadString());
  }

  private static void EnsureCapacity(List<LegacyNpc> npcs, WldReadLimits limits)
  {
    if (npcs.Count >= limits.MaxEntityCount)
    {
      throw new InvalidDataException("The WLD NPC count exceeds configured limits.");
    }
  }

  private static void ValidateCount(int count, WldReadLimits limits, string recordName)
  {
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException(
        $"The WLD {recordName} count exceeds configured limits.");
    }
  }
}
