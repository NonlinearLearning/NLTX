namespace NLTX.PlayerInputGameplay.Interaction;

public enum SmartInteractionCandidateKind
{
  PotionOfReturn,
  Projectile,
  Npc,
  Tile
}

public readonly record struct SmartInteractionCandidateSnapshot(
  SmartInteractionCandidateKind Kind,
  int TargetId,
  float DistanceFromCursor);
