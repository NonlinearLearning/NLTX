namespace Terraria.UiItemLocalization.Achievements;

public sealed class AchievementDefinitionRegistrationSystem
{
  public int Register(
    AchievementDefinitionCatalog catalog,
    IEnumerable<AchievementDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(definitions);

    int registered = 0;
    foreach (AchievementDefinition definition in definitions)
    {
      if (!catalog.Register(definition))
      {
        throw new InvalidOperationException(
          $"Achievement definition '{definition.Id.Value}' is already registered.");
      }

      registered++;
    }

    return registered;
  }
}
