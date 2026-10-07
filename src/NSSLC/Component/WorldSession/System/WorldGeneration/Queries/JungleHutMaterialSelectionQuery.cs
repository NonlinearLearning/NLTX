using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Selects the sourced jungle-hut material definition from one explicit roll.
/// </summary>
public static class JungleHutMaterialSelectionQuery
{
  public static JungleHutMaterialDefinition Select(
    in JungleHutMaterialSelectionRandomInput randomInput)
  {
    randomInput.Validate();

    ushort hutTileType = randomInput.HutSelectionRoll switch
    {
      0 => 119,
      1 => 120,
      2 => 158,
      3 => 175,
      _ => 45,
    };

    return JungleHutMaterialDefinitionQuery.Resolve(hutTileType);
  }
}
