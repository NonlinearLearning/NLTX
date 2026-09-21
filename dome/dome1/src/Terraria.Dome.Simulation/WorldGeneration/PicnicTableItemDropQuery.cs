namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PicnicTableItemDropQuery
{
  public static int ToItem(int style)
  {
    const int defaultItemId = 4064;
    const int styleOneItemId = 4065;

    return style == 1 ? styleOneItemId : defaultItemId;
  }
}
