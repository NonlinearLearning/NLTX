namespace Terraria.UiItemLocalization.Achievements;

public sealed class AchievementProgressState
{
  public AchievementProgressState(float maximumValue, string conditionName, string trackerType)
  {
    if (!float.IsFinite(maximumValue) || maximumValue < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumValue));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(conditionName);
    ArgumentException.ThrowIfNullOrWhiteSpace(trackerType);
    MaximumValue = maximumValue;
    ConditionName = conditionName;
    TrackerType = trackerType;
  }

  public float CurrentValue { get; private set; }

  public float MaximumValue { get; }

  public string ConditionName { get; }

  public string TrackerType { get; }

  public bool IsCompleted { get; private set; }

  public long Revision { get; private set; }

  internal void Apply(float value, bool completed)
  {
    CurrentValue = value;
    if (completed)
    {
      IsCompleted = true;
    }

    Revision++;
  }
}
