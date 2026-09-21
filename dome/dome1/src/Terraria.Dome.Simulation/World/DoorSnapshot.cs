namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct DoorSnapshot(
  int DoorId,
  int TileX,
  int TileY,
  bool IsOpen,
  long Revision,
  WorldSectionCoordinates Section,
  DoorObjectKind ObjectKind = DoorObjectKind.Door);
