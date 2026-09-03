namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TableItemDropQuery
{
  public static int ToItem(int style, bool secondType)
  {
    if (secondType)
    {
      return style switch
      {
        1 => 3948, 2 => 3974, 3 => 4162, 4 => 4183, 5 => 4204,
        6 => 4225, 7 => 4314, 8 => 4583, 9 => 5165, 10 => 5186,
        11 => 5207, 12 => 5565, 13 => 5618, 14 => 5706, 15 => 5729,
        16 => 5773, 17 => 5794, 18 => 5815, 19 => 5836, 20 => 5875,
        21 => 5894, 22 => 5915, 23 => 5949, 24 => 5971, 25 => 5992,
        26 => 6015, 27 => 6038, 28 => 6061, 29 => 6084, 30 => 6106,
        31 => 6128, _ => 3920
      };
    }

    if (style >= 1 && style <= 3)
    {
      return 637 + style;
    }

    if (style >= 4 && style <= 7)
    {
      return 823 + style;
    }

    if (style >= 15 && style <= 20)
    {
      return 1698 + style;
    }
    return style switch
    {
      8 => 917, 9 => 1144, 10 => 1397, 11 => 1400, 12 => 1403, 13 => 1460,
      14 => 1510, 21 => 1794, 22 => 1816, 23 => 1926, 24 => 2248, 25 => 2259,
      26 => 2532, 27 => 2550, 28 => 677, 29 => 2583, 30 => 2743, 31 => 2824,
      32 => 3153, 33 => 3155, 34 => 3154, _ => 32
    };
  }
}
