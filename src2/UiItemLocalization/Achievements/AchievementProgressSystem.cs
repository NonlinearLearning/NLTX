namespace Terraria.UiItemLocalization.Achievements;

public sealed class AchievementProgressSystem
{
  private readonly HashSet<string> _processedEventIds = new(StringComparer.Ordinal);

  public AchievementProgressUpdateResult Apply(
    AchievementProgressState state,
    AchievementDefinition definition,
    AchievementProgressCommand command)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(definition);

    if (!_processedEventIds.Add(command.EventId))
    {
      return AchievementProgressUpdateResult.DuplicateResult(state.Revision);
    }

    if (!definition.TryGetCondition(command.ConditionName, out AchievementConditionDefinition? condition))
    {
      return AchievementProgressUpdateResult.Rejected("unknown-condition", state.Revision);
    }

    if (state.ConditionName != condition.Name
      || state.MaximumValue != condition.MaximumValue
      || !string.Equals(state.TrackerType, condition.TrackerType, StringComparison.Ordinal))
    {
      return AchievementProgressUpdateResult.Rejected("progress-state-mismatch", state.Revision);
    }

    float appliedValue = Math.Clamp(command.Value, 0f, state.MaximumValue);
    bool wasCompleted = state.IsCompleted;
    bool reachesMaximum = appliedValue >= state.MaximumValue;
    state.Apply(appliedValue, reachesMaximum);
    return new AchievementProgressUpdateResult(
      Accepted: true,
      Duplicate: false,
      Completed: !wasCompleted && reachesMaximum,
      AppliedValue: appliedValue,
      state.Revision,
      FailureReason: null);
  }
}
