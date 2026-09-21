namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackSourceEligibility
{
  public QuickStackSourceEligibility(bool transferBlocked)
  {
    TransferBlocked = transferBlocked;
  }

  public bool TransferBlocked { get; }

  public bool IsEligible => !TransferBlocked;
}
