namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ToiletItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 4096;

    if (style >= 0 && style <= 31)
    {
      return 4096 + style;
    }

    return style switch
    {
      32 => 4141,
      33 => 4165,
      34 => 4186,
      35 => 4207,
      36 => 4228,
      37 => 4316,
      38 => 4586,
      39 => 4731,
      40 => 5168,
      41 => 5189,
      42 => 5210,
      43 => 5568,
      44 => 5621,
      45 => 5709,
      46 => 5732,
      47 => 5755,
      48 => 5774,
      49 => 5795,
      50 => 5816,
      51 => 5837,
      52 => 5855,
      53 => 5876,
      54 => 5895,
      55 => 5916,
      56 => 5950,
      57 => 5972,
      58 => 5993,
      59 => 6016,
      60 => 6039,
      61 => 6062,
      62 => 6085,
      63 => 6107,
      64 => 6129,
      _ => defaultItemId
    };
  }
}
