namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CoatingColorQuery
{
  public static CoatingColorValue GetColor(int coating)
  {
    return coating switch
    {
      1 => new(235, 170, byte.MaxValue, byte.MaxValue),
      2 => new(180, 245, byte.MaxValue, byte.MaxValue),
      _ => default
    };
  }
}
