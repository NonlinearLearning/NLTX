namespace Terraria.Dome.Simulation.WorldObjects.Placement;

public enum WorldObjectPlacementFailureCode
{
  None = 0,
  InvalidSequence = 1,
  DuplicateSequence = 2,
  InvalidOrigin = 3,
  UnsupportedObject = 4,
  InvalidStyle = 5,
  InvalidDirection = 6,
  InvalidSignText = 7,
  OwnerInactive = 8,
  ProjectileInactive = 9,
  OccupiedTile = 10,
  VersionConflict = 11,
  SectionVersionOverflow = 12,
  InvalidFootprint = 13,
  InvalidFrame = 14,
  MissingSupport = 15
}
