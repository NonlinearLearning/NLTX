namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldTransformationStateQuery
{
  public static WorldTransformationStateSnapshot FromActiveCount(int activeTransformations)
  {
    return new WorldTransformationStateSnapshot(activeTransformations);
  }
}
