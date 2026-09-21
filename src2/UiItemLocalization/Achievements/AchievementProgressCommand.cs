namespace Terraria.UiItemLocalization.Achievements;

public readonly record struct AchievementProgressCommand
{
  public AchievementProgressCommand(
    string eventId,
    PlayerEntityId playerId,
    LocalAchievementId achievementId,
    string conditionName,
    float value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(eventId);
    ArgumentException.ThrowIfNullOrWhiteSpace(conditionName);
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    EventId = eventId;
    PlayerId = playerId;
    AchievementId = achievementId;
    ConditionName = conditionName;
    Value = value;
  }

  public string EventId { get; }

  // crossSubsystemOwner: integration-review
  public PlayerEntityId PlayerId { get; }

  public LocalAchievementId AchievementId { get; }

  public string ConditionName { get; }

  public float Value { get; }
}
