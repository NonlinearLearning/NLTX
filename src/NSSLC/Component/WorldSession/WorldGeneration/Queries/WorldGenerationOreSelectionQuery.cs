using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class WorldGenerationOreSelectionQuery
{
  public static WorldGenerationOreSelectionSnapshot Snapshot(
    WorldGenerationOreSelectionComponent component)
  {
    return component.CreateSnapshot();
  }
}
