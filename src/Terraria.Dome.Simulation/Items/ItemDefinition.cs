using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items;

public readonly record struct ItemDefinition(
  ushort ItemType,
  int StackLimit,
  int HealthRestore = 0,
  int UseCooldownTicks = 0,
  ItemIdentityDefinition? Identity = null,
  ItemUseDefinition? Use = null,
  ItemCombatDefinition? Combat = null,
  ItemPlacementDefinition? Placement = null,
  ItemRecoveryDefinition? Recovery = null,
  ItemEquipmentDefinition? Equipment = null,
  int Width = 0,
  int Height = 0,
  int Value = 0,
  int Rarity = 0,
  ItemPrefixDefinition? Prefixes = null);
