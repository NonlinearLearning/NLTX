namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ForestBackgroundSetQuery
{
  public static ForestBackgroundSet Evaluate(int style)
  {
    return style switch
    {
      1 => new(7, 8, 50, 51, 52),
      2 => new(7, 8, 53, 54, 55),
      3 => new(7, 90, 91, -1, 92),
      31 => new(7, 90, 91, -1, 11),
      4 => new(93, 94, -1, -1, -1),
      5 => new(93, 94, -1, -1, 55),
      51 => new(93, 94, -1, -1, 11),
      6 => new(171, 172, 173, -1, -1),
      7 => new(176, 177, 178, -1, -1),
      71 => new(176, 177, 178, -1, 11),
      72 => new(176, 177, 178, -1, 52),
      73 => new(176, 177, 178, -1, 55),
      8 => new(179, 180, 184, -1, -1),
      9 => new(277, 278, 279, -1, -1),
      10 => new(280, 281, 282, -1, -1),
      11 => new(7, 331, 330, 329, 328),
      12 => new(7, 336, 335, 334, 333),
      13 => new(7, -1, 343, 342, 341),
      _ => new(7, 8, 9, 10, 11)
    };
  }
}
