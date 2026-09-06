namespace Terraria.Content;

public readonly record struct TileSolidityOverride(
  int TileTypeId,
  bool Solid,
  long ExpiresAtTick,
  TileOverrideReason Reason = TileOverrideReason.Unknown,
  TileOverridePhase OwnerPhase = TileOverridePhase.Unknown);
