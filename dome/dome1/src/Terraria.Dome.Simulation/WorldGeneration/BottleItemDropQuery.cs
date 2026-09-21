namespace Terraria.Dome.Simulation.WorldGeneration;

public static class BottleItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 31;

    return style switch
    {
      1 => 28,
      2 => 110,
      3 => 350,
      4 => 351,
      5 => 2234,
      6 => 2244,
      7 => 2257,
      8 => 2258,
      _ => defaultItemId
    };
  }
}
