namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcTownVariantSystem
{
  private const int ShimmerTownVariationIndex = 1;

  public bool IsShimmerVariant(int townNpcVariationIndex, bool supportsShimmerTownTransform)
  {
    return townNpcVariationIndex == ShimmerTownVariationIndex && supportsShimmerTownTransform;
  }
}
