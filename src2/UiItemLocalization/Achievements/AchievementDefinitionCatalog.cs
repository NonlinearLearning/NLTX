namespace Terraria.UiItemLocalization.Achievements;

public sealed class AchievementDefinitionCatalog
{
  private readonly Dictionary<LocalAchievementId, AchievementDefinition> _definitions = new();

  public int Count => _definitions.Count;

  public IReadOnlyCollection<AchievementDefinition> Definitions => _definitions.Values;

  public bool Register(AchievementDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    return _definitions.TryAdd(definition.Id, definition);
  }

  public bool TryGet(LocalAchievementId id, out AchievementDefinition? definition)
  {
    return _definitions.TryGetValue(id, out definition);
  }
}
