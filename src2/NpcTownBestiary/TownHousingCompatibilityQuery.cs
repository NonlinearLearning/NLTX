namespace Terraria.NpcTownBestiary;

public static class TownHousingCompatibilityQuery
{
  public static bool CanLiveTogether(NpcHousingCategory first, NpcHousingCategory second)
  {
    return first != second;
  }
}
