namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly struct DungeonFeaturePlacementPolicy
{
  public bool CanGenerateFeatureAt(int x, int y)
  {
    _ = x;
    _ = y;
    return true;
  }
}
