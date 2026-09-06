namespace Terraria.Player;

public sealed class PlayerInventoryState
{
  private readonly bool[] _chestStackEligibility = new bool[59];

  public PlayerInventoryState(IReadOnlyList<ItemEntityRef> mainInventory)
  {
    MainInventory = new ItemContainerState(mainInventory);
  }

  public ItemContainerState MainInventory { get; }

  public ItemContainerState PiggyBank { get; set; } = new(Array.Empty<ItemEntityRef>());

  public ItemContainerState Safe { get; set; } = new(Array.Empty<ItemEntityRef>());

  public ItemContainerState DefendersForge { get; set; } = new(Array.Empty<ItemEntityRef>());

  public ItemContainerState VoidVault { get; set; } = new(Array.Empty<ItemEntityRef>());

  public VoidVaultState VoidVaultFlags { get; set; }

  public ItemEntityRef TrashItem { get; set; }

  public IReadOnlyList<bool> ChestStackEligibility => _chestStackEligibility;

  public int SelectedSlotIndex { get; set; }

  public int LastHotbarSlotIndex { get; set; }

  public int? BufferedSelectedSlotIndex { get; set; }

  public int? OverriddenSelectedSlotIndex { get; set; }

  public ItemEntityRef HeldItem => (uint)SelectedSlotIndex < (uint)MainInventory.Slots.Count
    ? MainInventory.Slots[SelectedSlotIndex]
    : ItemEntityRef.None;
}
