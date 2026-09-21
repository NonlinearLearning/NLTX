namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackDestinationEligibility
{
  public QuickStackDestinationEligibility(bool locked, bool transferBlocked)
  {
    Locked = locked;
    TransferBlocked = transferBlocked;
  }

  public bool Locked { get; }

  public bool TransferBlocked { get; }

  public bool IsEligible => !Locked && !TransferBlocked;
}
