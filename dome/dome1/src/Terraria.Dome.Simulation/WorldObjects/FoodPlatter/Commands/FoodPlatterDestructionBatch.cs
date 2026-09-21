using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Commands;

public readonly record struct FoodPlatterDestructionBatch(
  long Sequence,
  int TileX,
  int TileY,
  int EntityId,
  bool RemoveTileEntity,
  ItemStack DroppedItem);
