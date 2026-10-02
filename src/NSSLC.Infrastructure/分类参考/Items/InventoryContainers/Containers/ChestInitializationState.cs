namespace Terraria.Items.InventoryContainers;

public sealed class ChestInitializationState
{
  public bool ItemsGotSet { get; private set; }

  public void MarkItemsSet()
  {
    ItemsGotSet = true;
  }
}
