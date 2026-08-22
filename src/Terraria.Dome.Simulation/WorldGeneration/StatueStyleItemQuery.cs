namespace Terraria.Dome.Simulation.WorldGeneration;

public static class StatueStyleItemQuery
{
  public static int ToItem(int style)
  {
    return style switch
    {
      0 => 360,
      1 => 52,
      43 => 1152,
      44 => 1153,
      45 => 1154,
      46 => 1408,
      47 => 1409,
      48 => 1410,
      49 => 1462,
      50 => 2672,
      >= 51 and <= 62 => 3651 + style - 51,
      >= 63 and <= 75 => 3708 + style - 63,
      76 => 4397,
      77 => 4360,
      78 => 4342,
      79 => 4466,
      80 => 5317,
      81 => 5318,
      82 => 5319,
      _ => 438 + style - 2
    };
  }
}
