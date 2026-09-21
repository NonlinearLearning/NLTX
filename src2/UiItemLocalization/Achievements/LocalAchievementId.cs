namespace Terraria.UiItemLocalization.Achievements;

// This is a local definition key until the cross-partition owner approves a persistent or network ID.
public readonly record struct LocalAchievementId
{
  public LocalAchievementId(string value)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(value);
    Value = value;
  }

  public string Value { get; }
}
