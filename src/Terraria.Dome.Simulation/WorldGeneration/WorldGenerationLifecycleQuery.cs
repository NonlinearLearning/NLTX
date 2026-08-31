namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldGenerationLifecycleQuery
{
  public static WorldGenerationLifecycleSnapshot FromLegacyFlags(
    bool generatingWorld,
    bool isGeneratingOrLoadingWorld)
  {
    return new WorldGenerationLifecycleSnapshot(generatingWorld, isGeneratingOrLoadingWorld);
  }
}
