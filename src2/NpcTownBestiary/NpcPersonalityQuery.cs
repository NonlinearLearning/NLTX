namespace Terraria.NpcTownBestiary;

public static class NpcPersonalityQuery
{
  public static NpcPersonalityEvaluationResult Evaluate(
    NpcPersonalityCatalog catalog,
    int npcType,
    PersonalityEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    if (!catalog.TryGet(npcType, out NpcPersonalityDefinition? definition))
    {
      return new NpcPersonalityEvaluationResult(false, NpcAffectionLevel.Neutral, null);
    }

    NpcBiomePreference? bestPreference = null;
    foreach (NpcBiomePreference preference in definition.BiomePreferences)
    {
      bool active = context.ActiveBiomeKeys.Any(key =>
        string.Equals(key, preference.Biome.LocalizationKey, StringComparison.OrdinalIgnoreCase));
      if (!active || (bestPreference.HasValue &&
        preference.Affection <= bestPreference.Value.Affection))
      {
        continue;
      }

      bestPreference = preference;
    }

    if (!bestPreference.HasValue)
    {
      return new NpcPersonalityEvaluationResult(true, NpcAffectionLevel.Neutral, null);
    }

    return new NpcPersonalityEvaluationResult(
      true,
      bestPreference.Value.Affection,
      bestPreference.Value.Biome);
  }
}
