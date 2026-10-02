namespace Terraria.Player;

public sealed class PlayerBuffComponent
{
  public const int MaximumSlotCount = 44;

  public BuffSlot[] Slots { get; } = new BuffSlot[MaximumSlotCount];

  // Buff immunity is distinct from damage immunity timers.
  public HashSet<ContentId<BuffDefinition>> ImmuneBuffTypes { get; } = [];
}
