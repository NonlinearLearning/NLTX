using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Resolves the sourced jungle-hut tile to its matching wall definition.
/// </summary>
public static class JungleHutMaterialDefinitionQuery
{
  public static JungleHutMaterialDefinition Resolve(ushort jungleHut)
  {
    ushort wallType = jungleHut switch
    {
      119 => 23,
      120 => 24,
      158 => 42,
      175 => 45,
      45 => 10,
      _ => 0
    };

    return new JungleHutMaterialDefinition(jungleHut, wallType);
  }
}
