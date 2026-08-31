namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyWallFrameCommitResult(
  bool Succeeded,
  int AppliedCount,
  long NextSequence,
  string? FailureReason)
{
  public static LegacyWallFrameCommitResult Failed(string reason)
  {
    return new LegacyWallFrameCommitResult(false, 0, 0, reason);
  }
}
