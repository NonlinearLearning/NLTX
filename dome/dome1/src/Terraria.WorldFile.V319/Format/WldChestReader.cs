using System;
using System.Collections.Generic;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldChestReader
{
  public static IReadOnlyList<LegacyChest> Read(
    int version,
    WldBinaryReader reader,
    LegacyWorldMetadata metadata,
    WldReadLimits limits)
  {
    const string sectionName = "chests";
    int recordIndex = -1;
    try
    {
      int chestCount = ReadInt16Count(reader, limits, "chest");
      int sharedSlotCount = version < 294
        ? ReadInt16Count(reader, limits, "chest slot")
        : 0;
      HashSet<(int X, int Y)> coordinates = [];
      List<LegacyChest> chests = new(chestCount);
      for (recordIndex = 0; recordIndex < chestCount; recordIndex++)
      {
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        ValidateCoordinates(x, y, metadata);
        if (!coordinates.Add((x, y)))
        {
          throw new InvalidDataException("A WLD chest coordinate is duplicated.");
        }

        string name = reader.ReadString();
        int slotCount = version >= 294
          ? ReadInt32Count(reader, limits, "chest slot")
          : sharedSlotCount;
        List<LegacyChestItem> items = new(slotCount);
        for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
        {
          short stack = reader.ReadInt16();
          if (stack < 0)
          {
            throw new InvalidDataException("A WLD chest item stack cannot be negative.");
          }

          int netId = 0;
          byte prefix = 0;
          if (stack > 0)
          {
            netId = reader.ReadInt32();
            prefix = reader.ReadByte();
          }

          items.Add(new LegacyChestItem(stack, netId, prefix));
        }

        chests.Add(new LegacyChest(x, y, name, items));
      }

      reader.RequireSectionEnd();
      return chests;
    }
    catch (InvalidDataException exception)
    {
      throw CreateException(version, sectionName, recordIndex, reader.Position, exception);
    }
  }

  private static InvalidDataException CreateException(
    int version,
    string sectionName,
    int recordIndex,
    long offset,
    InvalidDataException exception)
  {
    return new InvalidDataException(
      $"WLD read failed: version={version}, section={sectionName}, record={recordIndex}, " +
      $"offset={offset}. {exception.Message}",
      exception);
  }

  private static int ReadInt16Count(
    WldBinaryReader reader,
    WldReadLimits limits,
    string recordName)
  {
    short count = reader.ReadInt16();
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException(
        $"The WLD {recordName} count exceeds configured limits.");
    }

    return count;
  }

  private static int ReadInt32Count(
    WldBinaryReader reader,
    WldReadLimits limits,
    string recordName)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException(
        $"The WLD {recordName} count exceeds configured limits.");
    }

    return count;
  }

  private static void ValidateCoordinates(int x, int y, LegacyWorldMetadata metadata)
  {
    if (x < 0 || x >= metadata.Width || y < 0 || y >= metadata.Height)
    {
      throw new InvalidDataException("A WLD chest coordinate is outside the world.");
    }
  }
}
