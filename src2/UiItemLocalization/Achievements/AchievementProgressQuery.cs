namespace Terraria.UiItemLocalization.Achievements;

public static class AchievementProgressQuery
{
  public static AchievementProgressSnapshot CreateSnapshot(AchievementProgressState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return new AchievementProgressSnapshot(
      state.CurrentValue,
      state.MaximumValue,
      state.ConditionName,
      state.TrackerType,
      state.IsCompleted,
      state.Revision);
  }
}
