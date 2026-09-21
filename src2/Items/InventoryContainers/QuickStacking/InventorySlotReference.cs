namespace Terraria.Items.InventoryContainers;

public readonly record struct InventorySlotReference(string ContainerKey, int Slot)
{
  public bool IsValid => !string.IsNullOrWhiteSpace(ContainerKey) && Slot >= 0;
}
