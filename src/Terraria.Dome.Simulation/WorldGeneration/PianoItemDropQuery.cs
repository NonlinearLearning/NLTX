namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PianoItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 333;

    if (style >= 1 && style <= 3)
    {
      return 640 + style;
    }

    if (style >= 5 && style <= 7)
    {
      return 2245 + style - 5;
    }

    if (style >= 8 && style <= 10)
    {
      return 2254 + style - 8;
    }

    if (style >= 11 && style <= 20)
    {
      return 2376 + style - 11;
    }

    return style switch
    {
      4 => 919, 21 => 2531, 22 => 2548, 23 => 2565, 24 => 2580,
      25 => 2671, 26 => 2821, 27 => 3141, 28 => 3143, 29 => 3142,
      30 => 3915, 31 => 3916, 32 => 3944, 33 => 3971, 34 => 4158,
      35 => 4179, 36 => 4200, 37 => 4221, 38 => 4310, 39 => 4579,
      40 => 5161, 41 => 5182, 42 => 5203, 43 => 5561, 44 => 5614,
      45 => 5702, 46 => 5725, 47 => 5750, 48 => 5769, 49 => 5790,
      50 => 5811, 51 => 5832, 52 => 5851, 53 => 5871, 54 => 5891,
      55 => 5911, 56 => 5945, 57 => 5968, 58 => 5988, 59 => 6011,
      60 => 6034, 61 => 6057, 62 => 6080, 63 => 6102, 64 => 6124,
      _ => defaultItemId
    };
  }
}
