namespace Terraria.Player.Mount;

public readonly record struct DrillMountTickInput(
  float DiodeRotationTarget,
  float RotationStep,
  float OuterRingRotationDelta);

public readonly record struct DrillBeamReservationResult(
  bool Reserved,
  int BeamIndex,
  DrillMountRuntimeComponent.DrillTileTarget? Target,
  DrillMountRuntimeComponent.DrillBeamPurpose Purpose);
