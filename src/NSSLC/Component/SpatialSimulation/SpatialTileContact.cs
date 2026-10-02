namespace Terraria.SpatialSimulation;

// status: proposed
// resultId: SPATIAL.RESULT.TILE_CONTACT
// crossSubsystemOwner: integration-review
public readonly record struct SpatialTileContact(
  int X,
  int Y,
  bool BlocksMovement,
  bool IsWet,
  bool IsLavaWet,
  bool IsHoneyWet,
  bool IsShimmerWet);
