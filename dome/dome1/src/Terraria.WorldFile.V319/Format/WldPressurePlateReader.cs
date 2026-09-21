using System;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldPressurePlateReader
{
  public static LegacySectionDiagnostic? Read(
    WldBinaryReader reader,
    LegacyWorldMetadata metadata,
    WldReadLimits limits)
  {
    byte[] payload = reader.CopySectionBytes();
    int count = reader.ReadInt32();
    if (count < 0 || count > limits.MaxEntityCount)
    {
      throw new InvalidDataException("The WLD pressure-plate count exceeds configured limits.");
    }

    for (int index = 0; index < count; index++)
    {
      int x = reader.ReadInt32();
      int y = reader.ReadInt32();
      if (x < 0 || x >= metadata.Width || y < 0 || y >= metadata.Height)
      {
        throw new InvalidDataException("A WLD pressure-plate coordinate is outside the world.");
      }
    }

    reader.RequireSectionEnd();
    return count == 0
      ? null
      : new LegacySectionDiagnostic(
        6,
        reader.SectionStart,
        "Pressure-plate records were preserved for compatibility.",
        payload);
  }
}
