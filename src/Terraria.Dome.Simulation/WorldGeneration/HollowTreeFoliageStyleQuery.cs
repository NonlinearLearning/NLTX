namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HollowTreeFoliageStyleQuery
{
  public static int GetStyle(int hallowBackgroundStyle)
  {
    return hallowBackgroundStyle switch
    {
      4 => 19,
      2 or 3 => 20,
      _ => 3
    };
  }
}
