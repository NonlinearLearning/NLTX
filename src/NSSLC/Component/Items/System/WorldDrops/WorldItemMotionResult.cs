namespace Terraria.Items;

public readonly record struct WorldItemMotionResult(
  bool Applied,
  bool Changed,
  long Revision,
  WorldItemMotionRejectionReason RejectionReason)
{
  public static WorldItemMotionResult Rejected(
    long currentRevision,
    WorldItemMotionRejectionReason reason)
  {
    return new WorldItemMotionResult(
      Applied: false,
      Changed: false,
      Revision: currentRevision,
      RejectionReason: reason);
  }
}
