using System;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldCreativePowerReader
{
  public static LegacySectionDiagnostic? Read(WldBinaryReader reader)
  {
    byte[] payload = reader.CopySectionBytes();
    reader.Seek(reader.SectionEnd);
    reader.RequireSectionEnd();
    return payload.Length <= 1
      ? null
      : new LegacySectionDiagnostic(
        9,
        reader.SectionStart,
        "Creative-power records were preserved as opaque compatibility data.",
        payload);
  }
}
