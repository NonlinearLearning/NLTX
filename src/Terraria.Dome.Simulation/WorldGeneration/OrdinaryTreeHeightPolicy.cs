namespace Terraria.Dome.Simulation.WorldGeneration;

public static class OrdinaryTreeHeightPolicy
{
  private const int MaximumBaseHeight = 16;
  private const int MinimumBaseHeight = 5;

  public static OrdinaryTreeHeightResult Next(
    GenerationRandomState state,
    int treeHeightAddon)
  {
    (GenerationRandomState next, int baseHeight) = state.NextInclusive(
      MinimumBaseHeight,
      MaximumBaseHeight);
    return new OrdinaryTreeHeightResult(next, checked(baseHeight + treeHeightAddon));
  }
}
