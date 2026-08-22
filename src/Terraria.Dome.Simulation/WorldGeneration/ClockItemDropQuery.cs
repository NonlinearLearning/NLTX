namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ClockItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 359;

    if (style >= 1 && style <= 5)
    {
      return 2237 + style - 1;
    }

    if (style >= 8 && style <= 23)
    {
      return 2591 + style - 8;
    }

    return style switch
    {
      6 => 2560,
      7 => 2575,
      24 => 2809,
      25 => 3126,
      26 => 3128,
      27 => 3127,
      28 => 3898,
      29 => 3899,
      30 => 3900,
      31 => 3901,
      32 => 3902,
      33 => 3940,
      34 => 3966,
      35 => 4154,
      36 => 4175,
      37 => 4196,
      38 => 4217,
      39 => 4306,
      40 => 4575,
      41 => 5157,
      42 => 5178,
      43 => 5199,
      44 => 5557,
      45 => 5610,
      46 => 5698,
      47 => 5721,
      48 => 5746,
      49 => 5764,
      50 => 5785,
      51 => 5806,
      52 => 5827,
      53 => 5847,
      54 => 5866,
      55 => 5887,
      56 => 5906,
      57 => 5940,
      58 => 5963,
      59 => 5983,
      60 => 6006,
      61 => 6029,
      62 => 6052,
      63 => 6075,
      64 => 6097,
      65 => 6119,
      _ => defaultItemId
    };
  }
}
