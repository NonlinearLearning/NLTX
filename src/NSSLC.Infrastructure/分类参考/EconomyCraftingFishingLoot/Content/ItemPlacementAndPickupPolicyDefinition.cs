namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class ItemPlacementAndPickupPolicyDefinition
{
  public ItemPlacementAndPickupPolicyDefinition(
    int foodWidth,
    int foodHeight,
    int wallPlacementUseTime,
    int pickupReplacementTime,
    int slotsRemainingBeforeEmergencyStacking)
  {
    ValidatePositive(foodWidth, nameof(foodWidth));
    ValidatePositive(foodHeight, nameof(foodHeight));
    ValidateNonNegative(wallPlacementUseTime, nameof(wallPlacementUseTime));
    ValidateNonNegative(pickupReplacementTime, nameof(pickupReplacementTime));
    ValidateNonNegative(
      slotsRemainingBeforeEmergencyStacking,
      nameof(slotsRemainingBeforeEmergencyStacking));

    FoodWidth = foodWidth;
    FoodHeight = foodHeight;
    WallPlacementUseTime = wallPlacementUseTime;
    PickupReplacementTime = pickupReplacementTime;
    SlotsRemainingBeforeEmergencyStacking = slotsRemainingBeforeEmergencyStacking;
  }

  public int FoodWidth { get; }

  public int FoodHeight { get; }

  public int WallPlacementUseTime { get; }

  public int PickupReplacementTime { get; }

  public int SlotsRemainingBeforeEmergencyStacking { get; }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  private static void ValidatePositive(int value, string parameterName)
  {
    if (value <= 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
