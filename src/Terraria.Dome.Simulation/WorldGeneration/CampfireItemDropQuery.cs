namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CampfireItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 966;

    if (style >= 1 && style <= 5)
    {
      return 3045 + style;
    }

    if (style >= 8 && style <= 13)
    {
      return 4681 + style;
    }

    return style switch
    {
      6 => 3723,
      7 => 3724,
      14 => 5299,
      15 => 5357,
      _ => defaultItemId
    };
  }
}
