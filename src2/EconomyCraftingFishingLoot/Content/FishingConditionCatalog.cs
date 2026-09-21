using System.Collections.Frozen;
using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Fishing;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class FishingConditionCatalog
{
  private readonly FrozenDictionary<string, FishingConditionDelegateDefinition>
    _definitionsByName;
  private readonly ImmutableArray<FishingConditionDelegateDefinition> _definitions;

  public FishingConditionCatalog(
    IEnumerable<FishingConditionDelegateDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    List<FishingConditionDelegateDefinition> snapshot = definitions.ToList();
    Dictionary<string, FishingConditionDelegateDefinition> byName =
      new(StringComparer.Ordinal);
    foreach (FishingConditionDelegateDefinition definition in snapshot)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!byName.TryAdd(definition.Name, definition))
      {
        throw new ArgumentException(
          $"Fishing condition '{definition.Name}' is registered more than once.",
          nameof(definitions));
      }
    }

    _definitions = snapshot.ToImmutableArray();
    _definitionsByName = byName.ToFrozenDictionary(StringComparer.Ordinal);
  }

  public int Count => _definitions.Length;

  public ImmutableArray<FishingConditionDelegateDefinition> Definitions => _definitions;

  public bool TryGet(
    string name,
    out FishingConditionDelegateDefinition definition)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    return _definitionsByName.TryGetValue(name, out definition!);
  }

  public static FishingConditionCatalog CreateDefault()
  {
    FishingConditionDisplayMetadata displayMetadata =
      new(canBeSkippedForDisplay: false);
    return new FishingConditionCatalog(
    [
      new FishingConditionDelegateDefinition(
        "HardMode",
        context => context.IsHardMode,
        displayMetadata),
      new FishingConditionDelegateDefinition(
        "EarlyMode",
        context => !context.IsHardMode,
        displayMetadata),
      new FishingConditionDelegateDefinition(
        "Junk",
        context => context.Junk,
        displayMetadata),
      new FishingConditionDelegateDefinition(
        "Crate",
        context => context.Crate,
        displayMetadata),
      new FishingConditionDelegateDefinition(
        "AnyEnemies",
        context => context.RolledEnemySpawn > 0,
        displayMetadata),
      new FishingConditionDelegateDefinition(
        "DidNotUseCombatBook",
        context => !context.CombatBookWasUsed,
        displayMetadata)
    ]);
  }
}
