using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public static class InfectionGenerationSystem
{
  public static InfectionAlignmentComponent CreateAlignment(
    long generationId,
    bool crimsonLeft,
    bool drunkWorld,
    bool getGoodWorld,
    bool remixWorld)
  {
    return new InfectionAlignmentComponent(
      generationId,
      crimsonLeft,
      drunkWorld && getGoodWorld && !remixWorld);
  }
}
