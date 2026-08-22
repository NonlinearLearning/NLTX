namespace Terraria.Dome.Simulation.WorldGeneration;

public static class UndergroundTreeHeightPolicy
{
  private const int MaximumBaseHeight = 14;
  private const int MinimumBaseHeight = 5;

  public static UndergroundTreeHeightResult Next(
    GenerationRandomState state,
    int treeHeightAddon)
  {
    (GenerationRandomState next, int baseHeight) = state.NextInclusive(
      MinimumBaseHeight,
      MaximumBaseHeight);
    return new UndergroundTreeHeightResult(next, checked(baseHeight + treeHeightAddon));
  }
}
