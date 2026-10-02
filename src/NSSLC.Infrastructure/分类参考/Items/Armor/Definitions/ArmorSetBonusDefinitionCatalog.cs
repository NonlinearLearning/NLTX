namespace Terraria.NonAuthoritative.ContentDefinitions;

public enum ArmorSetPartType
{
  None,
  Head,
  Body,
  Legs
}

public readonly record struct ArmorSetPartSet(int Head, int Body, int Legs);

public sealed class ArmorSetBonusBuilder
{
  private readonly List<ArmorSetPartSet> _sets = new();

  public ArmorSetBonusBuilder(string textKey, ArmorSetPartType primaryPart)
  {
    if (string.IsNullOrWhiteSpace(textKey))
    {
      throw new ArgumentException("Armor set bonuses require a text key.", nameof(textKey));
    }

    TextKey = textKey;
    PrimaryPart = primaryPart;
  }

  public string TextKey { get; }

  public ArmorSetPartType PrimaryPart { get; }

  public ArmorSetBonusBuilder Set(int head, int body, int legs)
  {
    ValidateItemType(head);
    ValidateItemType(body);
    ValidateItemType(legs);
    _sets.Add(new ArmorSetPartSet(head, body, legs));
    return this;
  }

  public ArmorSetBonusBuilder Set(
    IReadOnlyList<int>? headOptions,
    IReadOnlyList<int>? bodyOptions,
    IReadOnlyList<int>? legOptions)
  {
    int[] heads = headOptions is { Count: > 0 } ? headOptions.ToArray() : new[] { 0 };
    int[] bodies = bodyOptions is { Count: > 0 } ? bodyOptions.ToArray() : new[] { 0 };
    int[] legs = legOptions is { Count: > 0 } ? legOptions.ToArray() : new[] { 0 };
    foreach (int head in heads)
    {
      foreach (int body in bodies)
      {
        foreach (int leg in legs)
        {
          Set(head, body, leg);
        }
      }
    }

    return this;
  }

  public ArmorSetBonusDefinition Build()
  {
    return new ArmorSetBonusDefinition(TextKey, PrimaryPart, _sets);
  }

  private static void ValidateItemType(int itemType)
  {
    if (itemType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemType));
    }
  }
}

public sealed class ArmorSetBonusDefinition
{
  internal ArmorSetBonusDefinition(
    string textKey,
    ArmorSetPartType primaryPart,
    IReadOnlyList<ArmorSetPartSet> sets)
  {
    TextKey = textKey;
    PrimaryPart = primaryPart;
    Sets = sets.ToArray();
  }

  public string TextKey { get; }

  public ArmorSetPartType PrimaryPart { get; }

  public IReadOnlyList<ArmorSetPartSet> Sets { get; }

  public int Head => Sets.Count == 0 ? 0 : Sets[0].Head;

  public int Body => Sets.Count == 0 ? 0 : Sets[0].Body;

  public int Legs => Sets.Count == 0 ? 0 : Sets[0].Legs;
}

public readonly record struct ArmorSetQueryContext(int HeadItem, int BodyItem, int LegItem);

public readonly record struct ArmorSetQueryResult(int ItemsNeeded, int ItemsFound)
{
  public bool Complete => ItemsNeeded == ItemsFound;
}

public sealed class ArmorSetBonusDefinitionCatalog
{
  private readonly List<ArmorSetBonusDefinition> _definitions = new();

  public IReadOnlyList<ArmorSetBonusDefinition> Definitions => _definitions;

  internal void Add(ArmorSetBonusDefinition definition)
  {
    _definitions.Add(definition);
  }
}

public static class ArmorSetBonusDefinitionRegistrationSystem
{
  public static void Register(
    ArmorSetBonusDefinitionCatalog catalog,
    ArmorSetBonusDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(definition);
    catalog.Add(definition);
  }
}

public static class ArmorSetQualificationQuery
{
  public static ArmorSetQueryResult Evaluate(
    ArmorSetBonusDefinition definition,
    ArmorSetQueryContext context)
  {
    ArgumentNullException.ThrowIfNull(definition);
    ArmorSetPartSet set = definition.Sets.Count == 0
      ? new ArmorSetPartSet(0, 0, 0)
      : definition.Sets[0];
    int needed = 0;
    int found = 0;
    Count(set.Head, context.HeadItem, ref needed, ref found);
    Count(set.Body, context.BodyItem, ref needed, ref found);
    Count(set.Legs, context.LegItem, ref needed, ref found);
    return new ArmorSetQueryResult(needed, found);
  }

  private static void Count(int neededItem, int testedItem, ref int needed, ref int found)
  {
    if (neededItem == 0)
    {
      return;
    }

    needed++;
    if (neededItem == testedItem)
    {
      found++;
    }
  }
}
