namespace Terraria.WorldGeneration.Passes;

public static class WorldSizeCatalogDefinition
{
  public const int AutoCreateDisabledValue = 0;
  public const int AutoCreateSmallValue = 1;
  public const int AutoCreateMediumValue = 2;
  public const int AutoCreateLargeValue = 3;

  public const int SmallIndex = 0;
  public const int MediumIndex = 1;
  public const int LargeIndex = 2;

  public const int SmallWidth = 4200;
  public const int SmallHeight = 1200;
  public const int MediumWidth = 6400;
  public const int MediumHeight = 1800;
  public const int LargeWidth = 8400;
  public const int LargeHeight = 2400;

  public static WorldSizeProfile Small { get; } =
    new(SmallWidth, SmallHeight);

  public static WorldSizeProfile Medium { get; } =
    new(MediumWidth, MediumHeight);

  public static WorldSizeProfile Large { get; } =
    new(LargeWidth, LargeHeight);

  public static WorldSizeProfile FromLegacyIndex(int size)
  {
    return size switch
    {
      SmallIndex => Small,
      MediumIndex => Medium,
      LargeIndex => Large,
      _ => throw new ArgumentOutOfRangeException(nameof(size)),
    };
  }

  public static bool TryFromAutoCreateValue(
    int autoCreateValue,
    out WorldSizeProfile profile)
  {
    switch (autoCreateValue)
    {
      case AutoCreateSmallValue:
        profile = Small;
        return true;
      case AutoCreateMediumValue:
        profile = Medium;
        return true;
      case AutoCreateLargeValue:
        profile = Large;
        return true;
      case AutoCreateDisabledValue:
        profile = default;
        return false;
      default:
        throw new ArgumentOutOfRangeException(nameof(autoCreateValue));
    }
  }

  public static WorldSizeProfile FromCommandValue(int commandValue)
  {
    if (!TryFromAutoCreateValue(commandValue, out WorldSizeProfile profile))
    {
      throw new ArgumentOutOfRangeException(nameof(commandValue));
    }

    return profile;
  }

  public static int GetLegacyIndexForWidth(int width)
  {
    if (width <= SmallWidth)
    {
      return SmallIndex;
    }

    return width <= MediumWidth ? MediumIndex : LargeIndex;
  }
}
