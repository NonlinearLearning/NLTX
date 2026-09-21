namespace Terraria.UiItemLocalization.Achievements;

public sealed class AchievementConditionDefinition
{
  public AchievementConditionDefinition(string name, float maximumValue, string trackerType)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(trackerType);
    if (!float.IsFinite(maximumValue) || maximumValue < 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumValue));
    }

    Name = name;
    MaximumValue = maximumValue;
    TrackerType = trackerType;
  }

  public string Name { get; }

  public float MaximumValue { get; }

  public string TrackerType { get; }
}
