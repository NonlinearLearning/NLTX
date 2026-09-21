namespace Terraria.UiItemLocalization.Achievements;

public sealed record AchievementConditionSaveSnapshot(
  string AchievementId,
  string ConditionName,
  float Value,
  bool IsCompleted,
  long Revision);
