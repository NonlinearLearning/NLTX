namespace NLTX.PlayerInputGameplay.Player;

public sealed class LocalPlayerSelectionComponent
{
  public int SlotIndex { get; private set; } = -1;

  public bool HasSelection => SlotIndex >= 0;

  public void Select(int slotIndex, int capacity)
  {
    if (slotIndex < 0 || slotIndex >= capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(slotIndex));
    }

    SlotIndex = slotIndex;
  }

  public void Clear()
  {
    SlotIndex = -1;
  }
}
