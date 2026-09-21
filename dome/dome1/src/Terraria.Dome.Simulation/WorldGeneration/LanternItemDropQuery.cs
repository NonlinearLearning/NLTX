namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LanternItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 136;

    if (style == 0)
    {
      return defaultItemId;
    }

    if (style == 7)
    {
      return 1431;
    }

    if (style == 8)
    {
      return 1808;
    }

    if (style == 9)
    {
      return 1859;
    }

    if (style < 10)
    {
      return 1389 + style;
    }

    if (style >= 10 && style <= 21)
    {
      return 2032 + style - 10;
    }

    if (style >= 22 && style <= 25)
    {
      return 2145 + style - 22;
    }

    return style switch
    {
      26 => 2226,
      27 => 2530,
      28 => 2546,
      29 => 2564,
      30 => 2579,
      31 => 2641,
      32 => 2642,
      33 => 2820,
      34 => 3138,
      35 => 3140,
      36 => 3139,
      37 => 3891,
      38 => 3943,
      39 => 3970,
      40 => 4157,
      41 => 4178,
      42 => 4199,
      43 => 4220,
      44 => 4309,
      45 => 4578,
      46 => 5160,
      47 => 5181,
      48 => 5202,
      49 => 5560,
      50 => 5613,
      51 => 5701,
      52 => 5724,
      53 => 5749,
      54 => 5768,
      55 => 5789,
      56 => 5810,
      57 => 5831,
      58 => 5850,
      59 => 5870,
      60 => 5890,
      61 => 5910,
      62 => 5944,
      63 => 5967,
      64 => 5987,
      65 => 6010,
      66 => 6033,
      67 => 6056,
      68 => 6079,
      69 => 6101,
      70 => 6123,
      _ => defaultItemId
    };
  }
}
