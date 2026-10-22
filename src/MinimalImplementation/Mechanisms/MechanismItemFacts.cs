using Terraria.Items;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Narrow item facts consumed by P06. Inventory ownership and mutation remain P04 concerns.
/// </summary>
public interface IMechanismItemFacts
{
  bool IsValid(ItemState item);

  bool FitsKiteAnchor(short itemType);

  bool FitsCritterAnchor(short itemType);
}

/// <summary>Version4 bounds and anchor eligibility supplied at the P06 boundary.</summary>
public sealed class Version4MechanismItemFacts : IMechanismItemFacts
{
  private readonly IReadOnlySet<short> _kiteItems;
  private readonly IReadOnlyDictionary<short, short> _critterNpcByItem;

  public Version4MechanismItemFacts(
    IReadOnlySet<short> kiteItems,
    IReadOnlyDictionary<short, short> critterNpcByItem)
  {
    ArgumentNullException.ThrowIfNull(kiteItems);
    ArgumentNullException.ThrowIfNull(critterNpcByItem);
    _kiteItems = new HashSet<short>(kiteItems);
    _critterNpcByItem = new Dictionary<short, short>(critterNpcByItem);
    if (_kiteItems.Any(itemType => itemType <= 0) ||
        _critterNpcByItem.Any(pair => pair.Key <= 0 || pair.Value <= 0))
    {
      throw new ArgumentException("Anchor item facts must use positive Version4 ids.");
    }
  }

  public bool IsValid(ItemState item)
  {
    if (item.IsEmpty)
    {
      return item.Type == 0 && item.Stack == 0 && item.Prefix == 0;
    }

    return item.Type is > 0 and < 6147 &&
      item.Stack > 0 && item.Stack <= 9999 &&
      item.Prefix < 98;
  }

  public bool FitsKiteAnchor(short itemType)
  {
    return _kiteItems.Contains(itemType);
  }

  public bool FitsCritterAnchor(short itemType)
  {
    return _critterNpcByItem.TryGetValue(itemType, out short npcType) && npcType > 0;
  }

  public bool TryGetCritterNpcType(short itemType, out short npcType)
  {
    return _critterNpcByItem.TryGetValue(itemType, out npcType);
  }
}
