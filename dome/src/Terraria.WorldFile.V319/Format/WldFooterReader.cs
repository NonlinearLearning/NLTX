using System;
using System.IO;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319.Format;

public static class WldFooterReader
{
  public static void Read(int version, WldBinaryReader reader, LegacyWorldMetadata metadata)
  {
    try
    {
      if (!reader.ReadBoolean())
      {
        throw new InvalidDataException("The WLD footer is marked invalid.");
      }

      string name = reader.ReadString();
      int worldId = reader.ReadInt32();
      if (!StringComparer.Ordinal.Equals(name, metadata.Name) || worldId != metadata.WorldId)
      {
        throw new InvalidDataException("The WLD footer identity does not match the header.");
      }

      reader.RequireSectionEnd();
    }
    catch (InvalidDataException exception)
    {
      throw new InvalidDataException(
        $"WLD read failed: version={version}, section=footer, record=-1, " +
        $"offset={reader.Position}. {exception.Message}",
        exception);
    }
  }
}
