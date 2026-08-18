using System;
using System.Collections.Generic;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldSignReader
{
  public static IReadOnlyList<LegacySign> Read(
    int version,
    WldBinaryReader reader,
    LegacyWorldMetadata metadata,
    WldReadLimits limits)
  {
    const string sectionName = "signs";
    int recordIndex = -1;
    try
    {
      short count = reader.ReadInt16();
      if (count < 0 || count > limits.MaxEntityCount)
      {
        throw new InvalidDataException("The WLD sign count exceeds configured limits.");
      }

      HashSet<(int X, int Y)> coordinates = [];
      List<LegacySign> signs = new(count);
      for (recordIndex = 0; recordIndex < count; recordIndex++)
      {
        string text = reader.ReadString();
        int x = reader.ReadInt32();
        int y = reader.ReadInt32();
        if (x < 0 || x >= metadata.Width || y < 0 || y >= metadata.Height)
        {
          throw new InvalidDataException("A WLD sign coordinate is outside the world.");
        }

        if (!coordinates.Add((x, y)))
        {
          throw new InvalidDataException("A WLD sign coordinate is duplicated.");
        }

        signs.Add(new LegacySign(x, y, text));
      }

      reader.RequireSectionEnd();
      return signs;
    }
    catch (InvalidDataException exception)
    {
      throw new InvalidDataException(
        $"WLD read failed: version={version}, section={sectionName}, record={recordIndex}, " +
        $"offset={reader.Position}. {exception.Message}",
        exception);
    }
  }
}
