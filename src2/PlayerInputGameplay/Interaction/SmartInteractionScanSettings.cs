using System.Numerics;

namespace NLTX.PlayerInputGameplay.Interaction;

public readonly record struct SmartInteractionScanSettings(
  int PlayerSlot,
  bool DemandOnlyZeroDistanceTargets,
  bool FullInteraction,
  Vector2 Mouse,
  int LeftX,
  int RightX,
  int LeftY,
  int RightY);
