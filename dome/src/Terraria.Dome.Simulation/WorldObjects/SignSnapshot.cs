using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct SignSnapshot(
  int SignId,
  int TileX,
  int TileY,
  string Text,
  long Revision,
  WorldSectionCoordinates Section);
