namespace Terraria.Dome.Simulation.WorldGeneration;

public static class MossSelectionPolicy
{
  public static MossSelectionResult Next(GenerationRandomState state, bool justNeon = false)
  {
    (GenerationRandomState next, int neonIndex) = state.NextExclusive(4);
    ushort neonTileType = neonIndex switch
    {
      0 => 539,
      1 => 536,
      2 => 534,
      _ => 625
    };
    if (justNeon)
    {
      return new MossSelectionResult(next, neonTileType, -1, -1, -1);
    }

    (next, int first) = next.NextExclusive(5);
    (next, int second) = NextDifferent(next, first);
    (next, int third) = NextDifferent(next, first, second);
    return new MossSelectionResult(next, neonTileType, first, second, third);
  }

  private static (GenerationRandomState State, int Value) NextDifferent(
    GenerationRandomState state,
    int first,
    int? second = null)
  {
    GenerationRandomState next = state;
    int value;
    do
    {
      (next, value) = next.NextExclusive(5);
    }
    while (value == first || value == second);

    return (next, value);
  }
}
