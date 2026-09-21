using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration.Definitions;

public static class TorchDefinitionRegistry
{
  private static readonly IReadOnlyList<TorchDefinition> DefinitionsById =
    new List<TorchDefinition>
    {
      new TorchDefinition(0, 6, true),
      new TorchDefinition(1, 59, false),
      new TorchDefinition(2, 60, false),
      new TorchDefinition(3, 61, false),
      new TorchDefinition(4, 62, false),
      new TorchDefinition(5, 63, false),
      new TorchDefinition(6, 64, false),
      new TorchDefinition(7, 65, true),
      new TorchDefinition(8, 75, false),
      new TorchDefinition(9, 135, true),
      new TorchDefinition(10, 158, false),
      new TorchDefinition(11, 169, false),
      new TorchDefinition(12, 156, false),
      new TorchDefinition(13, 234, true),
      new TorchDefinition(14, 66, false),
      new TorchDefinition(15, 242, false),
      new TorchDefinition(16, 293, true),
      new TorchDefinition(17, 294, false),
      new TorchDefinition(18, 295, true),
      new TorchDefinition(19, 296, true),
      new TorchDefinition(20, 297, true),
      new TorchDefinition(21, 298, true),
      new TorchDefinition(22, 307, true),
      new TorchDefinition(23, 310, true)
    }.AsReadOnly();

  public static IReadOnlyList<TorchDefinition> Definitions => DefinitionsById;

  public static IReadOnlyList<TorchDefinition> RegisterDefaults()
  {
    return DefinitionsById;
  }

  public static bool TryGet(int torchId, out TorchDefinition definition)
  {
    if (torchId < 0 || torchId >= DefinitionsById.Count)
    {
      definition = default;
      return false;
    }

    definition = DefinitionsById[torchId];
    return true;
  }
}
