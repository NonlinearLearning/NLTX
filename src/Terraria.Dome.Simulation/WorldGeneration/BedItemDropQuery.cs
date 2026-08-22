namespace Terraria.Dome.Simulation.WorldGeneration;

public static class BedItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 224;

    if (style >= 1 && style <= 3)
    {
      return style + 643;
    }

    if (style >= 5 && style <= 8)
    {
      return 1465 + style;
    }

    if (style >= 9 && style <= 12)
    {
      return 1710 + style;
    }

    if (style >= 13 && style <= 18)
    {
      return 2066 + style - 13;
    }

    return style switch
    {
      4 => 920,
      19 => 2139,
      20 => 2140,
      21 => 2231,
      22 => 2520,
      23 => 2538,
      24 => 2553,
      25 => 2568,
      26 => 2669,
      27 => 2811,
      28 => 3162,
      29 => 3164,
      30 => 3163,
      31 => 3897,
      32 => 3932,
      33 => 3959,
      34 => 4146,
      35 => 4167,
      36 => 4188,
      37 => 4209,
      38 => 4299,
      39 => 4567,
      40 => 5149,
      41 => 5170,
      42 => 5191,
      43 => 5549,
      44 => 5602,
      45 => 5690,
      46 => 5713,
      47 => 5740,
      48 => 5757,
      49 => 5778,
      50 => 5799,
      51 => 5820,
      52 => 5841,
      53 => 5859,
      54 => 5880,
      55 => 5899,
      56 => 5933,
      57 => 5956,
      58 => 5976,
      59 => 5999,
      60 => 6022,
      61 => 6045,
      62 => 6068,
      63 => 6091,
      64 => 6112,
      _ => defaultItemId
    };
  }
}
