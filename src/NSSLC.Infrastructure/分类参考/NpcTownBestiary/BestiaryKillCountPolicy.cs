namespace Terraria.NpcTownBestiary;

public static class BestiaryKillCountPolicy
{
  public const int PositiveCap = 999999999;

  public static int Clamp(int value)
  {
    return Math.Clamp(value, 0, PositiveCap);
  }
}
