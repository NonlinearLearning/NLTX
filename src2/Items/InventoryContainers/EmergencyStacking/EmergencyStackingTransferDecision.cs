namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingTransferDecision
{
  public EmergencyStackingTransferDecision(bool hasOwnership, int amount)
  {
    HasOwnership = hasOwnership;
    Amount = amount;
  }

  public bool HasOwnership { get; }

  public int Amount { get; }
}
