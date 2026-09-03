using System;
using System.IO;

namespace Terraria.WorldFile.V319.Format;

public sealed record WldReadLimits
{
  public const int DefaultMaximumEntityCount = 100_000;
  public const long DefaultMaximumFileBytes = 512L * 1024L * 1024L;
  public const int DefaultMaximumStringByteLength = 1_048_576;
  public const int DefaultMaximumTileCount = 20_160_000;
  public const int DefaultMaximumWorldHeight = 2_400;
  public const int DefaultMaximumWorldWidth = 8_400;

  public static WldReadLimits Default { get; } = new();

  public int MaxEntityCount { get; init; } = DefaultMaximumEntityCount;

  public long MaxFileBytes { get; init; } = DefaultMaximumFileBytes;

  public int MaxStringByteLength { get; init; } = DefaultMaximumStringByteLength;

  public int MaxTileCount { get; init; } = DefaultMaximumTileCount;

  public int MaxWorldHeight { get; init; } = DefaultMaximumWorldHeight;

  public int MaxWorldWidth { get; init; } = DefaultMaximumWorldWidth;

  public void Validate()
  {
    if (MaxFileBytes <= 0 || MaxStringByteLength < 0 || MaxTileCount <= 0 ||
        MaxWorldWidth <= 0 || MaxWorldHeight <= 0 || MaxEntityCount < 0)
    {
      throw new InvalidDataException("The configured WLD read limits are invalid.");
    }
  }

  public void ValidateWorldDimensions(int width, int height)
  {
    Validate();
    if (width <= 0 || width > MaxWorldWidth || height <= 0 || height > MaxWorldHeight)
    {
      throw new InvalidDataException("The WLD world dimensions exceed configured limits.");
    }

    long tileCount = (long)width * height;
    if (tileCount > MaxTileCount)
    {
      throw new InvalidDataException("The WLD world tile count exceeds configured limits.");
    }
  }
}
