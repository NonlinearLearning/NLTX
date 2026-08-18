using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct ChestSnapshot(
  int ChestId,
  int TileX,
  int TileY,
  PlayerHandle? Opener,
  ItemStack[] Slots,
  long Revision,
  WorldSectionCoordinates Section);
