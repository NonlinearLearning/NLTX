using System;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldTownManagerReader
{
  public static LegacySectionDiagnostic? Read(WldBinaryReader reader, WldReadLimits limits)
  {
    byte[] payload = reader.CopySectionBytes();
    int count = reader.ReadInt32();
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException("The WLD town-room count exceeds configured limits.");
    }

    for (int index = 0; index < count; index++)
    {
      _ = reader.ReadInt32();
      _ = reader.ReadInt32();
      _ = reader.ReadInt32();
    }

    reader.RequireSectionEnd();
    return count == 0
      ? null
      : new LegacySectionDiagnostic(
        7,
        reader.SectionStart,
        "Town-manager records were preserved for compatibility.",
        payload);
  }
}
