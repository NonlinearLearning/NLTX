namespace Terraria.Player;

public sealed class PlayerBuffComponent
{
  public const int MaximumSlotCount = 44;

  public BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];

  // Buff immunity is distinct from damage immunity timers.
  public HashSet<ContentId<BuffDefinition>> ImmuneBuffTypes { get; } = [];

  internal void SetNetworkBuff(ushort buffType, int durationTicks)
  {
    ContentId<BuffDefinition> type = new(buffType);
    for (int index = 0; index < Slots.Length; index++)
    {
      if (Slots[index].Type == type || Slots[index].IsEmpty)
      {
        Slots[index] = new BuffSlot(type, durationTicks);
        return;
      }
    }
  }
}
