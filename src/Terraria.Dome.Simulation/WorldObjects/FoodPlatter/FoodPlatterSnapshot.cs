using Terraria.Dome.Simulation.Items;

namespace Terraria.Dome.Simulation.WorldObjects.FoodPlatter;

public readonly record struct FoodPlatterSnapshot(
  int EntityId,
  int TileX,
  int TileY,
  bool Exists,
  ItemStack StoredItem);
