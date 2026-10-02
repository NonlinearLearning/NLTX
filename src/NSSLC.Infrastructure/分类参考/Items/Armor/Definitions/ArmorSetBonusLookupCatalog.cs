namespace Terraria.NonAuthoritative.ContentDefinitions;

public sealed class ArmorSetBonusLookupCatalog
{
  private readonly Dictionary<int, List<ArmorSetBonusDefinition>> _setsContaining = new();

  public IReadOnlyList<ArmorSetBonusDefinition> All { get; private set; } =
    Array.Empty<ArmorSetBonusDefinition>();

  public IReadOnlyList<ArmorSetBonusDefinition> GetSetsContaining(int itemType)
  {
    return _setsContaining.TryGetValue(itemType, out List<ArmorSetBonusDefinition>? values)
      ? values
      : Array.Empty<ArmorSetBonusDefinition>();
  }

  public bool TryGetCompleteSet(
    ArmorSetQueryContext context,
    out ArmorSetBonusDefinition? definition)
  {
    foreach (ArmorSetBonusDefinition candidate in All)
    {
      if (ArmorSetQualificationQuery.Evaluate(candidate, context).Complete)
      {
        definition = candidate;
        return true;
      }
    }

    definition = null;
    return false;
  }

  internal void Replace(IReadOnlyList<ArmorSetBonusDefinition> definitions)
  {
    All = definitions.ToArray();
    _setsContaining.Clear();
    foreach (ArmorSetBonusDefinition definition in All)
    {
      Add(definition.Head, definition);
      Add(definition.Body, definition);
      Add(definition.Legs, definition);
    }
  }

  private void Add(int itemType, ArmorSetBonusDefinition definition)
  {
    if (itemType == 0)
    {
      return;
    }

    if (!_setsContaining.TryGetValue(itemType, out List<ArmorSetBonusDefinition>? values))
    {
      values = new List<ArmorSetBonusDefinition>();
      _setsContaining.Add(itemType, values);
    }

    values.Add(definition);
  }
}

public static class ArmorSetBonusLookupBuildSystem
{
  public static void Rebuild(
    ArmorSetBonusLookupCatalog catalog,
    IEnumerable<ArmorSetBonusDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(definitions);
    catalog.Replace(definitions.ToArray());
  }
}
