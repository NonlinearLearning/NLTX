using Terraria.Relationships;

namespace Terraria.Items;

public readonly record struct CraftingMaterialReservation(
  EntityReference Item,
  int SourceSlot,
  int Quantity,
  ulong ExpectedStackKey);
