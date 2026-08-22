namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct WorldSizeProfile(int Width, int Height)
{
  public const int SmallIndex = 0;
  public const int MediumIndex = 1;
  public const int LargeIndex = 2;

  public const int SmallWidth = 4200;
  public const int SmallHeight = 1200;
  public const int MediumWidth = 6400;
  public const int MediumHeight = 1800;
  public const int LargeWidth = 8400;
  public const int LargeHeight = 2400;

  public static WorldSizeProfile FromLegacyIndex(int size)
  {
    return size switch
    {
      SmallIndex => new WorldSizeProfile(SmallWidth, SmallHeight),
      MediumIndex => new WorldSizeProfile(MediumWidth, MediumHeight),
      _ => new WorldSizeProfile(LargeWidth, LargeHeight)
    };
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
