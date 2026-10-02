namespace Terraria.NonAuthoritative.ContentDefinitions;

public interface IWorldRuleCondition
{
  bool Matches(WorldRuleSnapshot snapshot);
}

public sealed class RequiredWorldRuleCondition : IWorldRuleCondition
{
  private readonly string _ruleKey;

  public RequiredWorldRuleCondition(string ruleKey)
  {
    if (string.IsNullOrWhiteSpace(ruleKey))
    {
      throw new ArgumentException("A world rule condition requires a key.", nameof(ruleKey));
    }

    _ruleKey = ruleKey;
  }

  public bool Matches(WorldRuleSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    return snapshot.IsEnabled(_ruleKey);
  }
}

public sealed class WorldRuleSnapshot
{
  private readonly IReadOnlyDictionary<string, bool> _rules;

  public WorldRuleSnapshot(IReadOnlyDictionary<string, bool> rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    _rules = new Dictionary<string, bool>(rules, StringComparer.Ordinal);
  }

  public bool IsEnabled(string ruleKey)
  {
    return _rules.TryGetValue(ruleKey, out bool enabled) && enabled;
  }
}

public sealed record ItemVariantDefinition
{
  public ItemVariantDefinition(
    int itemType,
    int variantId,
    string conditionKey,
    IWorldRuleCondition condition)
  {
    if (itemType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemType));
    }

    if (variantId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(variantId));
    }

    if (string.IsNullOrWhiteSpace(conditionKey))
    {
      throw new ArgumentException("Variants require a stable condition key.", nameof(conditionKey));
    }

    ArgumentNullException.ThrowIfNull(condition);
    ItemType = itemType;
    VariantId = variantId;
    ConditionKey = conditionKey;
    Condition = condition;
  }

  public int ItemType { get; }

  public int VariantId { get; }

  public string ConditionKey { get; }

  public IWorldRuleCondition Condition { get; }
}

public readonly record struct ItemVariantSelectionResult(
  bool IsSelected,
  int VariantId);

public sealed class ItemVariantCatalog
{
  private readonly Dictionary<int, List<ItemVariantDefinition>> _variants = new();

  public IReadOnlyDictionary<int, IReadOnlyList<ItemVariantDefinition>> Definitions
  {
    get
    {
      return _variants.ToDictionary(
        pair => pair.Key,
        pair => (IReadOnlyList<ItemVariantDefinition>)pair.Value.ToArray());
    }
  }

  internal void Add(ItemVariantDefinition definition)
  {
    if (!_variants.TryGetValue(definition.ItemType, out List<ItemVariantDefinition>? variants))
    {
      variants = new List<ItemVariantDefinition>();
      _variants.Add(definition.ItemType, variants);
    }

    if (variants.Any(existing => existing.VariantId == definition.VariantId))
    {
      throw new InvalidOperationException("An item variant ID may only be registered once per item.");
    }

    variants.Add(definition);
  }

  internal bool TryGet(int itemType, out IReadOnlyList<ItemVariantDefinition> definitions)
  {
    if (_variants.TryGetValue(itemType, out List<ItemVariantDefinition>? values))
    {
      definitions = values;
      return true;
    }

    definitions = Array.Empty<ItemVariantDefinition>();
    return false;
  }
}

public static class ItemVariantRegistrationSystem
{
  public static void Register(
    ItemVariantCatalog catalog,
    ItemVariantDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(definition);
    catalog.Add(definition);
  }
}

public static class ItemVariantSelectionQuery
{
  public static ItemVariantSelectionResult Select(
    ItemVariantCatalog catalog,
    int itemType,
    WorldRuleSnapshot worldRules)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(worldRules);
    if (!catalog.TryGet(itemType, out IReadOnlyList<ItemVariantDefinition> definitions))
    {
      return new ItemVariantSelectionResult(false, 0);
    }

    foreach (ItemVariantDefinition definition in definitions)
    {
      if (definition.Condition.Matches(worldRules))
      {
        return new ItemVariantSelectionResult(true, definition.VariantId);
      }
    }

    return new ItemVariantSelectionResult(false, 0);
  }
}
