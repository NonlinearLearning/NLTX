namespace Terraria.Npc;

public readonly record struct NpcTargetSelectionResult(
  bool HasTarget,
  bool ShouldCommit,
  NpcTargetKind TargetKind,
  int LegacyTargetIndex,
  int SecondaryLegacySlot,
  NpcTargetGeometrySnapshot TargetGeometry,
  float Score,
  int Direction,
  int DirectionY,
  bool NetUpdateRequested)
{
  public bool IsPlayerTankPet => TargetKind == NpcTargetKind.PlayerTankPet;

  public static NpcTargetSelectionResult NoTarget(
    int direction,
    int directionY,
    bool shouldCommit)
  {
    return new NpcTargetSelectionResult(
      HasTarget: false,
      shouldCommit,
      TargetKind: NpcTargetKind.None,
      LegacyTargetIndex: -1,
      SecondaryLegacySlot: -1,
      TargetGeometry: default,
      Score: float.PositiveInfinity,
      direction,
      directionY,
      NetUpdateRequested: false);
  }
}
