namespace Terraria.Player;

public sealed class PlayerBuffSlotsComponent
{
  public const int MaximumSlotCount = 44;

  internal BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];

  internal void ReplaceNetworkSlots(IReadOnlyList<ushort> types, int durationTicks)
  {
    Array.Clear(Slots);
    for (int index = 0; index < types.Count; index++)
    {
      Slots[index] = new BuffSlot(
        new ContentId<BuffDefinition>(types[index]),
        durationTicks);
    }
  }

  public IReadOnlyList<BuffSlot> Snapshot()
  {
    return Array.AsReadOnly(Slots.ToArray());
  }
}
