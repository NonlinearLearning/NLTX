using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration.Definitions;

public static class TorchDefinitionRegistry
{
  private static readonly TorchDefinition[] DefinitionsById =
  [
    new(0, 6, true),
    new(1, 59, false),
    new(2, 60, false),
    new(3, 61, false),
    new(4, 62, false),
    new(5, 63, false),
    new(6, 64, false),
    new(7, 65, true),
    new(8, 75, false),
    new(9, 135, true),
    new(10, 158, false),
    new(11, 169, false),
    new(12, 156, false),
    new(13, 234, true),
    new(14, 66, false),
    new(15, 242, false),
    new(16, 293, true),
    new(17, 294, false),
    new(18, 295, true),
    new(19, 296, true),
    new(20, 297, true),
    new(21, 298, true),
    new(22, 307, true),
    new(23, 310, true)
  ];

  public static IReadOnlyList<TorchDefinition> Definitions => DefinitionsById;

  public static bool TryGet(int torchId, out TorchDefinition definition)
  {
    if (torchId < 0 || torchId >= DefinitionsById.Length)
    {
      definition = default;
      return false;
    }

    definition = DefinitionsById[torchId];
    return true;
  }
}
