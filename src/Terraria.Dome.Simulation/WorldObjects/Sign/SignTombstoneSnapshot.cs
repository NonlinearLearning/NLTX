using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldObjects.Sign;

public readonly record struct SignTombstoneSnapshot(
  int SignId,
  long Revision,
  SignTombstoneReason Reason,
  WorldSectionCoordinates Section);
