using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.WorldObjects.FoodPlatter;

public readonly record struct FoodPlatterDestructionResult(
  bool ShouldDestroy,
  int EntityId,
  int TileX,
  int TileY,
  ItemStack DroppedItem);
