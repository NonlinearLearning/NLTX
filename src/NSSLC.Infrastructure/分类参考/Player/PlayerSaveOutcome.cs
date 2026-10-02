using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Player;

public readonly record struct PlayerSaveOutcome(
  bool Committed,
  bool SkippedByPolicy,
  FilePlatformFailure Failure)
{
  public static PlayerSaveOutcome Success => new(true, false, FilePlatformFailure.None);

  public static PlayerSaveOutcome Skipped => new(false, true, FilePlatformFailure.None);

  public static PlayerSaveOutcome Rejected(FilePlatformFailure failure)
  {
    return new(false, false, failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.IoFailure)
      : failure);
  }
}
