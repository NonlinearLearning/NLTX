namespace Terraria.UiItemLocalization.Achievements;

public sealed record AchievementPresentationSnapshot(
  LocalAchievementId AchievementId,
  string Name,
  string Description,
  bool IsCompleted,
  float CurrentValue,
  float MaximumValue,
  int IconIndex,
  long ProgressRevision,
  long LanguageRevision);
