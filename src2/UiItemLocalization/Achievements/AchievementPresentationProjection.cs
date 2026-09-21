namespace Terraria.UiItemLocalization.Achievements;

public static class AchievementPresentationProjection
{
  public static AchievementPresentationSnapshot Create(
    AchievementDefinition definition,
    AchievementProgressState progress,
    IAchievementTextResolver textResolver,
    long languageRevision)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArgumentNullException.ThrowIfNull(progress);
    ArgumentNullException.ThrowIfNull(textResolver);
    return new AchievementPresentationSnapshot(
      definition.Id,
      textResolver.Resolve(definition.FriendlyNameKey),
      textResolver.Resolve(definition.DescriptionKey),
      progress.IsCompleted,
      progress.CurrentValue,
      progress.MaximumValue,
      definition.IconIndex,
      progress.Revision,
      languageRevision);
  }
}
