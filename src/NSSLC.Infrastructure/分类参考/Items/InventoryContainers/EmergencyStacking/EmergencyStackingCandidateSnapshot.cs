namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingCandidateSnapshot
{
  public EmergencyStackingCandidateSnapshot(
    string itemKey,
    ItemStackSnapshot item,
    int age,
    bool isOnScreen,
    int ownerPlayerIndex)
  {
    if (string.IsNullOrWhiteSpace(itemKey))
    {
      throw new ArgumentException("An item key is required.", nameof(itemKey));
    }

    ArgumentNullException.ThrowIfNull(item);
    if (age < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(age));
    }

    ItemKey = itemKey;
    Item = item;
    Age = age;
    IsOnScreen = isOnScreen;
    OwnerPlayerIndex = ownerPlayerIndex;
  }

  public string ItemKey { get; }

  public ItemStackSnapshot Item { get; }

  public int Age { get; }

  public bool IsOnScreen { get; }

  public int OwnerPlayerIndex { get; }
}
