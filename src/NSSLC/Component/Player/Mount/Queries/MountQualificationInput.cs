namespace Terraria.Player.Mount;

public readonly record struct MountQualificationInput(
  int PlayerWidth,
  int PlayerHeight,
  bool CanFitInRequestedSpace,
  bool IsWet,
  bool IsDripping,
  bool HasWetCollision,
  bool IsGrappling,
  bool MountCanUseHooks);
