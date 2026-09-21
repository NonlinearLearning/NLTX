namespace Terraria.UiItemLocalization.Achievements;

public readonly record struct AchievementProgressUpdateResult(
  bool Accepted,
  bool Duplicate,
  bool Completed,
  float AppliedValue,
  long Revision,
  string? FailureReason)
{
  public static AchievementProgressUpdateResult Rejected(
    string reason,
    long revision)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    return new AchievementProgressUpdateResult(
      Accepted: false,
      Duplicate: false,
      Completed: false,
      AppliedValue: 0f,
      revision,
      reason);
  }

  public static AchievementProgressUpdateResult DuplicateResult(long revision)
  {
    return new AchievementProgressUpdateResult(
      Accepted: false,
      Duplicate: true,
      Completed: false,
      AppliedValue: 0f,
      revision,
      FailureReason: "duplicate-event");
  }
}
