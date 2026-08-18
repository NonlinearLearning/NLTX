using System;
using System.IO;
using Terraria.WorldFile.V319.Format;
using Terraria.WorldFile.V319.Model;

namespace Terraria.WorldFile.V319;

public static class WldWorldReader
{
  public const int MaximumSupportedVersion = 319;

  public static LegacyWorldDocument Read(Stream input, WldReadLimits? limits = null)
  {
    if (input is null || !input.CanRead || !input.CanSeek)
    {
      throw new InvalidDataException("The WLD input stream must be readable and seekable.");
    }

    WldReadLimits effectiveLimits = limits ?? WldReadLimits.Default;
    using WldBinaryReader reader = new(input, effectiveLimits, 0, input.Length);
    int version = reader.ReadInt32();
    WldFormatVersion formatVersion = SelectFormatVersion(version);
    if (formatVersion == WldFormatVersion.LegacyV1ToV87)
    {
      return WldLegacyV1ToV87Reader.Read(version, reader, effectiveLimits);
    }

    return WldV88ToV319Reader.Read(version, reader, effectiveLimits);
  }

  public static WldFormatVersion SelectFormatVersion(int version)
  {
    if (version <= 0)
    {
      throw new InvalidDataException($"The WLD version {version} is invalid.");
    }

    if (version > MaximumSupportedVersion)
    {
      throw new InvalidDataException(
        $"The WLD version {version} exceeds the supported maximum {MaximumSupportedVersion}.");
    }

    return version <= 87
      ? WldFormatVersion.LegacyV1ToV87
      : WldFormatVersion.PointerTableV88ToV319;
  }
}
