namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-712, P09-743, P09-744
// crossSubsystemOwner: item payload and transfer ordering remain integration-review
public sealed class PlayerInventorySlotsComponent
{
  public const int MainInventorySlotCount = 59;

  public ItemEntityRef[] MainInventorySlots { get; } =
    new ItemEntityRef[MainInventorySlotCount];

  public bool[] InventoryChestStackMarkers { get; } =
    new bool[MainInventorySlotCount];

  public ItemEntityRef TrashItem { get; internal set; } = ItemEntityRef.None;
}
