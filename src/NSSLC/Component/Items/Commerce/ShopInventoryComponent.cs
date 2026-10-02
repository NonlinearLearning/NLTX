using System.Collections.Generic;

namespace Terraria.Items.Commerce;

public sealed class ShopInventoryComponent
{
  private readonly List<CommerceOffer> _offers;

  public ShopInventoryComponent(
    IReadOnlyList<CommerceOffer>? offers = null,
    long shopRevision = 0,
    long? restockAtTick = null,
    long? lastRestockTick = null)
  {
    _offers = offers is null
      ? []
      : new List<CommerceOffer>(offers);
    ShopRevision = shopRevision;
    RestockAtTick = restockAtTick;
    LastRestockTick = lastRestockTick;
  }

  public long ShopRevision;
  public long? RestockAtTick;
  public long? LastRestockTick;

  public IReadOnlyList<CommerceOffer> Offers => _offers;

  public bool IsRestockDueAt(long currentTick) =>
    RestockAtTick.HasValue &&
    currentTick >= RestockAtTick.Value;
}
