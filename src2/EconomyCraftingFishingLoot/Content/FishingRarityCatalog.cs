using System.Collections.Frozen;
using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Fishing;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class FishingRarityCatalog
{
  private readonly FrozenDictionary<string, FishingRarityConditionDefinition>
    _definitionsByName;
  private readonly ImmutableArray<FishingRarityConditionDefinition> _definitions;

  public FishingRarityCatalog(
    IEnumerable<FishingRarityConditionDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    List<FishingRarityConditionDefinition> snapshot = definitions.ToList();
    Dictionary<string, FishingRarityConditionDefinition> byName =
      new(StringComparer.Ordinal);
    foreach (FishingRarityConditionDefinition definition in snapshot)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!byName.TryAdd(definition.Name, definition))
      {
        throw new ArgumentException(
          $"Fishing rarity '{definition.Name}' is registered more than once.",
          nameof(definitions));
      }
    }

    _definitions = snapshot.ToImmutableArray();
    _definitionsByName = byName.ToFrozenDictionary(StringComparer.Ordinal);
  }

  public int Count => _definitions.Length;

  public ImmutableArray<FishingRarityConditionDefinition> Definitions => _definitions;

  public bool TryGet(
    string name,
    out FishingRarityConditionDefinition definition)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    return _definitionsByName.TryGetValue(name, out definition!);
  }

  public static FishingRarityCatalog CreateDefault()
  {
    return new FishingRarityCatalog(
    [
      new FishingRarityConditionDefinition(
        "Any",
        new FishingRarityPredicate(context => true),
        frequencyOfAppearanceForVisuals: 1f,
        hackedIsAny: true),
      new FishingRarityConditionDefinition(
        "Legendary",
        new FishingRarityPredicate(context => context.Legendary),
        frequencyOfAppearanceForVisuals: 0.1f,
        hackedIsAny: false),
      new FishingRarityConditionDefinition(
        "VeryRare",
        new FishingRarityPredicate(context => context.VeryRare),
        frequencyOfAppearanceForVisuals: 0.25f,
        hackedIsAny: false),
      new FishingRarityConditionDefinition(
        "Rare",
        new FishingRarityPredicate(context => context.Rare),
        frequencyOfAppearanceForVisuals: 0.4f,
        hackedIsAny: false),
      new FishingRarityConditionDefinition(
        "Uncommon",
        new FishingRarityPredicate(context => context.Uncommon),
        frequencyOfAppearanceForVisuals: 0.8f,
        hackedIsAny: false),
      new FishingRarityConditionDefinition(
        "Common",
        new FishingRarityPredicate(context => context.Common),
        frequencyOfAppearanceForVisuals: 1f,
        hackedIsAny: false),
      new FishingRarityConditionDefinition(
        "BombRarityOfNotLegendaryAndNotVeryRareAndUncommon",
        new FishingRarityPredicate(
          context => !context.Legendary &&
            !context.VeryRare &&
            context.Uncommon),
        frequencyOfAppearanceForVisuals: 0.6f,
        hackedIsAny: false),
      new FishingRarityConditionDefinition(
        "UncommonOrCommon",
        new FishingRarityPredicate(
          context => context.Uncommon || context.Common),
        frequencyOfAppearanceForVisuals: 1f,
        hackedIsAny: false)
    ]);
  }
}
