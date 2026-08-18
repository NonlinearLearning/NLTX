using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct ChestIdentityComponent(
  int ChestId,
  int TileX,
  int TileY,
  WorldSectionCoordinates Section);
