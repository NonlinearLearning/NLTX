using System;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldBestiaryReader
{
  public static LegacySectionDiagnostic? Read(WldBinaryReader reader, WldReadLimits limits)
  {
    byte[] payload = reader.CopySectionBytes();
    long sectionStart = reader.SectionStart;
    ReadKillCountEntries(reader, limits);
    _ = ReadStringEntries(reader, limits);
    _ = ReadStringEntries(reader, limits);
    reader.RequireSectionEnd();
    return payload.Length == sizeof(int) * 3
      ? null
      : new LegacySectionDiagnostic(
        8,
        sectionStart,
        "Bestiary records were preserved for compatibility.",
        payload);
  }

  private static void ReadKillCountEntries(WldBinaryReader reader, WldReadLimits limits)
  {
    int count = ReadCount(reader, limits);
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadString();
      _ = reader.ReadInt32();
    }
  }

  private static int ReadStringEntries(WldBinaryReader reader, WldReadLimits limits)
  {
    int count = ReadCount(reader, limits);
    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadString();
    }

    return count;
  }

  private static int ReadCount(WldBinaryReader reader, WldReadLimits limits)
  {
    int count = reader.ReadInt32();
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException("The WLD bestiary count exceeds configured limits.");
    }

    return count;
  }
}
