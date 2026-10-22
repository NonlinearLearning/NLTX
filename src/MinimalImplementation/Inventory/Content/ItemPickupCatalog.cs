namespace MinimalImplementation.Server.Inventory.Content;

/// <summary>Missing content remains explicit instead of silently using generic pickup rules.</summary>
public sealed class ItemPickupCatalog
{
  private readonly Dictionary<int, ItemPickupFacts> _facts;

  public ItemPickupCatalog(IReadOnlyDictionary<int, ItemPickupFacts> facts)
  {
    ArgumentNullException.ThrowIfNull(facts);
    if (facts.Any(entry => entry.Key <= 0 || !ItemContentFacts.IsKnownItemType(entry.Key)
      || entry.Value is null))
    {
      throw new ArgumentException("Pickup facts must describe known nonempty items.", nameof(facts));
    }

    _facts = new Dictionary<int, ItemPickupFacts>(facts);
  }

  public bool TryGet(int itemType, out ItemPickupFacts? facts)
  {
    return _facts.TryGetValue(itemType, out facts);
  }

  /// <summary>Verified initial content only; Host may supply a larger verified catalog.</summary>
  public static ItemPickupCatalog CreateVersion4Core()
  {
    // Complete v1.4.5.6 Item.cs SetDefaults: IronPickaxe, Dirt/Stone/Torch/Wood,
    // Wooden/FlamingArrow, MusketBall and copper/silver/gold/platinum coins.
    var facts = new Dictionary<int, ItemPickupFacts> {
      [1] = new(false, false, true, false, true, false, false),
      [2] = new(false, false, true, false, false, false, false),
      [3] = new(false, false, true, false, false, false, false),
      [8] = new(false, false, true, false, false, false, false),
      [9] = new(false, false, true, false, false, false, false),
      [40] = new(true, true, false, false, false, false, true),
      [41] = new(true, true, false, false, false, false, true),
      [97] = new(true, true, false, false, false, false, true),
      [71] = new(false, false, true, false, false, false, false),
      [72] = new(false, false, true, false, false, false, false),
      [73] = new(false, false, true, false, false, false, false),
      [74] = new(false, false, true, false, false, false, false)
    };

    // DefaultToMusicBox is consumable with useStyle 1 and no ammo, bait or paint.
    // CanFillEmptyAmmoSlot is true, but FitsAmmoSlot excludes this branch entirely.
    for (int itemType = 1; itemType < ItemContentFacts.ItemTypeCount; itemType++)
    {
      if (ItemContentFacts.IsPlacedMusicBox(itemType))
      {
        facts.Add(itemType, new ItemPickupFacts(false, true, true, false, false, false, false));
      }
    }

    return new ItemPickupCatalog(facts);
  }
}
