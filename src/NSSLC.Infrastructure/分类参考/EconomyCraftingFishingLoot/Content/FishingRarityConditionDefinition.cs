using NLTX.EconomyCraftingFishingLoot.Fishing;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class FishingRarityConditionDefinition
{
  public FishingRarityConditionDefinition(
    string name,
    FishingRarityPredicate predicate,
    float frequencyOfAppearanceForVisuals,
    bool hackedIsAny)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentNullException.ThrowIfNull(predicate);
    if (float.IsNaN(frequencyOfAppearanceForVisuals) ||
      float.IsInfinity(frequencyOfAppearanceForVisuals) ||
      frequencyOfAppearanceForVisuals < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(frequencyOfAppearanceForVisuals));
    }

    Name = name;
    Predicate = predicate;
    FrequencyOfAppearanceForVisuals = frequencyOfAppearanceForVisuals;
    HackedIsAny = hackedIsAny;
  }

  public string Name { get; }

  public FishingRarityPredicate Predicate { get; }

  public float FrequencyOfAppearanceForVisuals { get; }

  public bool HackedIsAny { get; }

  public bool Matches(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return Predicate.Matches(context);
  }
}
