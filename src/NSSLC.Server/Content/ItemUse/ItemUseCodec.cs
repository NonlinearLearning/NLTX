using System.Text.Json;
using System.Text.Json.Serialization;
using Terraria.Item;
using Terraria.Lvcs;

namespace NSSLC.Server.Content.ItemUse;

/// <summary>
/// Loads item use profiles from JSON files into an immutable catalog.
/// </summary>
/// <remarks>
/// Parsing validates only what a single entry can answer: field presence, enum names, numeric
/// ranges. Duplicate ids and cross entry rules belong to projection, because they need the whole
/// batch. Detail goes to the diagnostic sink; failure is signalled by throwing
/// <see cref="LvcsContentException" /> so that the loader owns the single conversion point.
/// </remarks>
internal sealed class ItemUseCodec : ILvcsCodec<ItemUsePrototype, ItemUseDefinitionCatalog>
{
  /// <summary>Family id used by the item use family.</summary>
  public static readonly LvcsFamilyId FamilyId = new("itemUse");

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
  };

  public LvcsFamilyId Family => FamilyId;

  public IReadOnlyList<string> Extensions { get; } = [".json"];

  public IReadOnlyList<ItemUsePrototype> Parse(
      ILvcsUnit unit,
      LvcsDiagnosticSink diagnostics)
  {
    ArgumentNullException.ThrowIfNull(unit);
    ArgumentNullException.ThrowIfNull(diagnostics);

    List<ItemUsePrototype>? prototypes;
    try
    {
      prototypes = JsonSerializer.Deserialize<List<ItemUsePrototype>>(
          unit.Payload.Span, JsonOptions);
    }
    catch (JsonException exception)
    {
      throw new LvcsContentException(
          $"Could not parse item use JSON: {exception.Message}", exception);
    }

    if (prototypes is null || prototypes.Count == 0)
    {
      throw new LvcsContentException("File contains no item use entries.");
    }

    List<ItemUsePrototype> accepted = [];
    bool rejected = false;
    for (int index = 0; index < prototypes.Count; index++)
    {
      ItemUsePrototype? prototype = prototypes[index];
      if (prototype is null)
      {
        diagnostics.Error(
            Family, id: null, field: null,
            message: $"Null entry at index {index}.", source: unit.Location);
        rejected = true;
        continue;
      }

      if (IsEntryValid(prototype, unit.Location, diagnostics))
      {
        accepted.Add(prototype);
      }
      else
      {
        rejected = true;
      }
    }

    if (rejected)
    {
      throw new LvcsContentException("One or more item use entries were rejected.");
    }

    return accepted;
  }

  public ItemUseDefinitionCatalog Project(
      IReadOnlyList<ItemUsePrototype> raws,
      LvcsDiagnosticSink diagnostics)
  {
    ArgumentNullException.ThrowIfNull(raws);
    ArgumentNullException.ThrowIfNull(diagnostics);

    ItemUseDefinitionCatalog catalog = new();
    HashSet<ItemId> seen = [];
    bool rejected = false;

    foreach (ItemUsePrototype prototype in raws)
    {
      if (!TryParseItemId(prototype.Id, out ItemId itemId))
      {
        diagnostics.Error(
            Family, prototype.Id, nameof(prototype.Id), $"Unknown item id '{prototype.Id}'.");
        rejected = true;
        continue;
      }

      if (!seen.Add(itemId))
      {
        diagnostics.Error(
            Family, prototype.Id, nameof(prototype.Id), "Item use entry is declared twice.");
        rejected = true;
        continue;
      }

      if (!TryParseItemId(prototype.UseAmmo, out ItemId useAmmo))
      {
        diagnostics.Error(
            Family,
            prototype.Id,
            nameof(prototype.UseAmmo),
            $"Unknown item id '{prototype.UseAmmo}'.");
        rejected = true;
        continue;
      }

      // ConsumeAmmo and UseAmmo are independent fields; RequiresAmmo combines them and is
      // therefore false exactly in the broken case this guard must catch.
      if (prototype.ConsumeAmmo && useAmmo == ItemId.None)
      {
        diagnostics.Error(
            Family,
            prototype.Id,
            nameof(prototype.UseAmmo),
            "Entry consumes ammunition but useAmmo is None.");
        rejected = true;
        continue;
      }

      ItemUseDefinition definition = new(
          itemId,
          prototype.UseTime,
          prototype.UseAnimation,
          prototype.AutoReuse,
          prototype.Channel,
          useAmmo,
          prototype.ConsumeAmmo,
          prototype.ManaCost);

      if (!definition.IsValid)
      {
        diagnostics.Error(
            Family,
            prototype.Id,
            field: null,
            message: "Use timing or mana cost is out of range.");
        rejected = true;
        continue;
      }

      catalog.Register(definition);
    }

    if (rejected)
    {
      throw new LvcsContentException("One or more item use entries were rejected.");
    }

    if (catalog.Count == 0)
    {
      throw new LvcsContentException("Family defines no entries.");
    }

    catalog.Seal();
    return catalog;
  }

  private bool IsEntryValid(
      ItemUsePrototype prototype,
      string location,
      LvcsDiagnosticSink diagnostics)
  {
    if (string.IsNullOrWhiteSpace(prototype.Id))
    {
      diagnostics.Error(
          Family, id: null, field: nameof(prototype.Id),
          message: "Entry has an empty id.", source: location);
      return false;
    }

    if (!string.Equals(prototype.Type, FamilyId.Value, StringComparison.Ordinal))
    {
      diagnostics.Error(
          Family,
          prototype.Id,
          nameof(prototype.Type),
          message: $"Entry has type '{prototype.Type}' but expected '{FamilyId.Value}'.",
          source: location);
      return false;
    }

    if (!TryParseItemId(prototype.Id, out _))
    {
      diagnostics.Error(
          Family, prototype.Id, nameof(prototype.Id),
          message: $"Unknown item id '{prototype.Id}'.", source: location);
      return false;
    }

    if (!TryParseItemId(prototype.UseAmmo, out _))
    {
      diagnostics.Error(
          Family, prototype.Id, nameof(prototype.UseAmmo),
          message: $"Unknown item id '{prototype.UseAmmo}'.", source: location);
      return false;
    }

    return true;
  }

  private static bool TryParseItemId(string value, out ItemId itemId)
  {
    itemId = ItemId.None;
    if (string.IsNullOrWhiteSpace(value))
    {
      return false;
    }

    return Enum.TryParse(value, ignoreCase: false, out itemId) && ItemIdRules.IsKnown(itemId);
  }
}
