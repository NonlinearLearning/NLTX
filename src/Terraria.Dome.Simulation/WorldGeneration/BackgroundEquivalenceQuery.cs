namespace Terraria.Dome.Simulation.WorldGeneration;

public static class BackgroundEquivalenceQuery
{
  public static bool AreEquivalent(int oldBackground, int newBackground)
  {
    return oldBackground switch
    {
      3 or 31 => newBackground is 3 or 31,
      5 or 51 => newBackground is 5 or 51,
      7 or 71 or 72 or 73 => newBackground is 7 or 71 or 72 or 73,
      _ => oldBackground == newBackground
    };
  }
}
