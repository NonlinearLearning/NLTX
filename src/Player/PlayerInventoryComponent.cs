namespace Terraria.Player;

public sealed class PlayerInventoryComponent
{
  public const int MainInventorySlotCount = 59;

  public ItemEntityRef[] MainInventory { get; } =
    new ItemEntityRef[MainInventorySlotCount];

  public bool[] InventoryChestStackEligibility { get; } =
    new bool[MainInventorySlotCount];

  public PlayerContainerRef Bank { get; set; }

  public PlayerContainerRef Bank2 { get; set; }

  public PlayerContainerRef Bank3 { get; set; }

  public PlayerContainerRef Bank4 { get; set; }

  public VoidVaultState VoidVaultState { get; set; }

  public ItemEntityRef TrashItem { get; set; }

  public int SelectedSlotIndex { get; set; }

  public int LastHotbarSlotIndex { get; set; }

  public int? BufferedSelectedSlotIndex { get; set; }

  public int? OverriddenSelectedSlotIndex { get; set; }
}
