using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemVariantDefinition(
  ushort VariantId,
  ushort SourceItemType,
  ushort ReplacementItemType,
  IReadOnlyList<ItemDropCondition>? Conditions = null,
  bool OneTime = false);
