namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class ItemEventPricingDefinition
{
  public ItemEventPricingDefinition(
    int shadowOrbPrice,
    int dungeonPrice,
    int queenBeePrice,
    int hellPrice,
    int eclipsePrice,
    int eclipsePostPlanteraPrice,
    int eclipseMothronPrice)
  {
    ValidateNonNegative(shadowOrbPrice, nameof(shadowOrbPrice));
    ValidateNonNegative(dungeonPrice, nameof(dungeonPrice));
    ValidateNonNegative(queenBeePrice, nameof(queenBeePrice));
    ValidateNonNegative(hellPrice, nameof(hellPrice));
    ValidateNonNegative(eclipsePrice, nameof(eclipsePrice));
    ValidateNonNegative(eclipsePostPlanteraPrice, nameof(eclipsePostPlanteraPrice));
    ValidateNonNegative(eclipseMothronPrice, nameof(eclipseMothronPrice));

    ShadowOrbPrice = shadowOrbPrice;
    DungeonPrice = dungeonPrice;
    QueenBeePrice = queenBeePrice;
    HellPrice = hellPrice;
    EclipsePrice = eclipsePrice;
    EclipsePostPlanteraPrice = eclipsePostPlanteraPrice;
    EclipseMothronPrice = eclipseMothronPrice;
  }

  public int ShadowOrbPrice { get; }

  public int DungeonPrice { get; }

  public int QueenBeePrice { get; }

  public int HellPrice { get; }

  public int EclipsePrice { get; }

  public int EclipsePostPlanteraPrice { get; }

  public int EclipseMothronPrice { get; }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
