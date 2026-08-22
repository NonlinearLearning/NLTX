namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SinkItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 2827;

    if (style >= 0 && style <= 28)
    {
      return defaultItemId + style;
    }

    return style switch
    {
      29 => 3147, 30 => 3149, 31 => 3148, 32 => 3896, 33 => 3946,
      34 => 3972, 35 => 4160, 36 => 4181, 37 => 4202, 38 => 4223,
      39 => 4312, 40 => 4581, 41 => 5163, 42 => 5184, 43 => 5205,
      44 => 5563, 45 => 5616, 46 => 5704, 47 => 5727, 48 => 5752,
      49 => 5771, 50 => 5792, 51 => 5813, 52 => 5834, 53 => 5853,
      54 => 5873, 55 => 5892, 56 => 5913, 57 => 5947, 58 => 5969,
      59 => 5990, 60 => 6013, 61 => 6036, 62 => 6059, 63 => 6082,
      64 => 6104, 65 => 6126,
      _ => defaultItemId
    };
  }
}
