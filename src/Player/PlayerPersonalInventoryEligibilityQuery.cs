namespace Terraria.Player;

public static class PlayerPersonalInventoryEligibilityQuery
{
  public static bool CanTakeItemToPersonalInventory(
    in PlayerItemSpaceSnapshot snapshot)
  {
    return snapshot.CanTakeItem && !snapshot.ItemIsGoingToVoidVault;
  }
}
