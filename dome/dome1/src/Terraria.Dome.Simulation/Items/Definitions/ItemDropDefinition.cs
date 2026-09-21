using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemDropCondition(
  string Key,
  int MinimumValue = 0,
  bool Invert = false);

public readonly record struct ItemDropDefinition(
  ushort ItemType,
  int MinimumQuantity = 1,
  int MaximumQuantity = 1,
  int Weight = 1,
  bool ExpertOnly = false,
  bool MasterOnly = false,
  IReadOnlyList<ItemDropCondition>? Conditions = null,
  int ChainId = -1);
