namespace Terraria.Player;

public sealed class PlayerStatusEffectSnapshot
{
  public PlayerStatusEffectSnapshot(IReadOnlyList<BuffSlot> slots)
  {
    ArgumentNullException.ThrowIfNull(slots);
    Slots = Array.AsReadOnly(slots.ToArray());
  }

  public IReadOnlyList<BuffSlot> Slots { get; }

  public int ActiveCount => Slots.Count(slot => !slot.IsEmpty);
}
