namespace Terraria.UiItemLocalization.Achievements;

public sealed class AchievementDefinition
{
  public AchievementDefinition(
    LocalAchievementId id,
    string name,
    string friendlyNameKey,
    string descriptionKey,
    int iconIndex,
    IEnumerable<AchievementConditionDefinition> conditions)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentException.ThrowIfNullOrWhiteSpace(friendlyNameKey);
    ArgumentException.ThrowIfNullOrWhiteSpace(descriptionKey);
    if (iconIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(iconIndex));
    }

    ArgumentNullException.ThrowIfNull(conditions);
    List<AchievementConditionDefinition> copiedConditions = conditions.ToList();
    if (copiedConditions.Count == 0)
    {
      throw new ArgumentException("An achievement requires at least one condition.", nameof(conditions));
    }

    if (copiedConditions.GroupBy(condition => condition.Name, StringComparer.Ordinal).Any(group => group.Count() > 1))
    {
      throw new ArgumentException("Achievement condition names must be unique.", nameof(conditions));
    }

    Id = id;
    Name = name;
    FriendlyNameKey = friendlyNameKey;
    DescriptionKey = descriptionKey;
    IconIndex = iconIndex;
    Conditions = copiedConditions.AsReadOnly();
  }

  public LocalAchievementId Id { get; }

  public string Name { get; }

  public string FriendlyNameKey { get; }

  public string DescriptionKey { get; }

  public int IconIndex { get; }

  public IReadOnlyList<AchievementConditionDefinition> Conditions { get; }

  public bool TryGetCondition(string name, out AchievementConditionDefinition? condition)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    condition = Conditions.FirstOrDefault(candidate =>
      string.Equals(candidate.Name, name, StringComparison.Ordinal));
    return condition is not null;
  }
}
