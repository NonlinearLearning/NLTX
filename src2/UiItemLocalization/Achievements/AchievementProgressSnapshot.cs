namespace Terraria.UiItemLocalization.Achievements;

public readonly record struct AchievementProgressSnapshot(
  float CurrentValue,
  float MaximumValue,
  string ConditionName,
  string TrackerType,
  bool IsCompleted,
  long Revision);
