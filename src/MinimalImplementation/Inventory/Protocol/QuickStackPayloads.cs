namespace MinimalImplementation.Server.Inventory.Protocol;

public sealed record QuickStackRequestPayload
{
  public IReadOnlyList<short> SlotIds { get; }
  public bool SmartStack { get; }

  public QuickStackRequestPayload(IReadOnlyCollection<short> slotIds, bool smartStack)
  {
    ArgumentNullException.ThrowIfNull(slotIds);
    SlotIds = Array.AsReadOnly(slotIds.ToArray());
    SmartStack = smartStack;
  }
}

public sealed record QuickStackBlockedChestPayload
{
  public IReadOnlyList<ushort> ChestIds { get; }

  public QuickStackBlockedChestPayload(IReadOnlyCollection<ushort> chestIds)
  {
    ArgumentNullException.ThrowIfNull(chestIds);
    ChestIds = Array.AsReadOnly(chestIds.ToArray());
  }
}
