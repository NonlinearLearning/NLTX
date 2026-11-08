namespace Terraria.Item;

/// <summary>Server-side use profile of one item kind, as Version4 keeps on <c>Item</c>.</summary>
/// <remarks>
/// Version4 reads <c>useTime</c>, <c>useAnimation</c>, <c>autoReuse</c>, <c>channel</c>,
/// <c>useAmmo</c> and <c>mana</c> straight off the item instance seeded from the item table. The
/// server keeps them in one immutable record registered per item id, so a use never depends on
/// client-supplied numbers.
/// </remarks>
public sealed record ItemUseDefinition(
    ItemId ItemId,
    int UseTime,
    int UseAnimation,
    bool AutoReuse,
    bool Channel,
    ItemId UseAmmo,
    bool ConsumeAmmo,
    int ManaCost)
{
  public bool RequiresAmmo => UseAmmo != ItemId.None && ConsumeAmmo;

  public bool IsValid => UseTime > 0 && UseAnimation > 0 && ManaCost >= 0;
}
