namespace Terraria.Player;

public sealed class PlayerBuffState
{
  private readonly BuffSlot[] _slots;

  public PlayerBuffState(IReadOnlyList<BuffSlot> slots)
  {
    _slots = slots.ToArray();
  }

  public IReadOnlyList<BuffSlot> Slots => _slots;

  public IReadOnlySet<ContentId<BuffDefinition>> ImmuneBuffTypes { get; internal set; } =
    new HashSet<ContentId<BuffDefinition>>();

  public int MaximumSlotCount => _slots.Length;

  public int ActiveBuffCount => _slots.Count(slot => !slot.IsEmpty);

  public bool HasFreeSlot => _slots.Any(slot => slot.IsEmpty);
}
