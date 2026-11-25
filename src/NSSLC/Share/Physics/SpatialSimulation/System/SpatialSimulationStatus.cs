namespace Terraria.Share.Physics;

public enum SpatialSimulationStatus : byte
{
  Applied = 0,
  Rejected = 1,
  Conflict = 2,
  NotReady = 3
}
