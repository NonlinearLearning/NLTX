using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Calculates the Version4 infection-aligned surface material definition.
/// </summary>
public static class SurfaceMaterialDefinitionQuery
{
  public static SurfaceMaterialDefinition ApplyInfectionFlip(
    SurfaceMaterialDefinition definition,
    bool flipInfections)
  {
    if (!flipInfections)
    {
      return definition;
    }

    return new SurfaceMaterialDefinition(
      CrimsonStoneWall: definition.EbonStoneWall,
      CrimsonStone: definition.EbonStone,
      EbonStoneWall: definition.CrimsonStoneWall,
      EbonStone: definition.CrimsonStone,
      MossTile: definition.MossTile,
      MossWall: definition.MossWall);
  }
}
