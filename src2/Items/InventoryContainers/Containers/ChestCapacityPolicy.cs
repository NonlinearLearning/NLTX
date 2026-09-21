namespace Terraria.Items.InventoryContainers;

public static class ChestCapacityPolicy
{
  public const float StackRange = 600f;

  public const int MaxChestTypes = 52;

  public const int MaxChestTypes2 = 38;

  public const int MaxDresserTypes = 65;

  public const int DefaultMaxItems = 40;

  public const int AbsoluteMaxItemsWeCanEverReachInAChestForNow = 200;

  public const int AbsoluteMaxItems = AbsoluteMaxItemsWeCanEverReachInAChestForNow;

  public static bool IsValid(int capacity)
  {
    return capacity is >= 1 and <= AbsoluteMaxItemsWeCanEverReachInAChestForNow;
  }
}
