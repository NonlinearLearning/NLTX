namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public sealed class ShopMoodWeightsDefinition
{
  public const float LikeValue = 0.94f;

  public const float DislikeValue = 1.06f;

  public const float LoveValue = 0.88f;

  public const float HateValue = 1.12f;

  public float LikeMultiplier => LikeValue;

  public float DislikeMultiplier => DislikeValue;

  public float LoveMultiplier => LoveValue;

  public float HateMultiplier => HateValue;
}
