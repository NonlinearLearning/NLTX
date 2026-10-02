namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class ItemUseTimingConsumptionDefinition
{
  public ItemUseTimingConsumptionDefinition(
    int holdStyle,
    int useStyle,
    bool channel,
    bool accessory,
    int useAnimation,
    int useTime,
    bool potion,
    bool consumable,
    bool autoReuse,
    bool useTurn,
    bool noUseGraphic,
    bool noMelee,
    bool noWet,
    bool shootsEveryUse,
    int reuseDelay)
  {
    ValidateNonNegative(holdStyle, nameof(holdStyle));
    ValidateNonNegative(useStyle, nameof(useStyle));
    ValidateNonNegative(useAnimation, nameof(useAnimation));
    ValidateNonNegative(useTime, nameof(useTime));
    ValidateNonNegative(reuseDelay, nameof(reuseDelay));

    HoldStyle = holdStyle;
    UseStyle = useStyle;
    Channel = channel;
    Accessory = accessory;
    UseAnimation = useAnimation;
    UseTime = useTime;
    Potion = potion;
    Consumable = consumable;
    AutoReuse = autoReuse;
    UseTurn = useTurn;
    NoUseGraphic = noUseGraphic;
    NoMelee = noMelee;
    NoWet = noWet;
    ShootsEveryUse = shootsEveryUse;
    ReuseDelay = reuseDelay;
  }

  public int HoldStyle { get; }

  public int UseStyle { get; }

  public bool Channel { get; }

  public bool Accessory { get; }

  public int UseAnimation { get; }

  public int UseTime { get; }

  public bool Potion { get; }

  public bool Consumable { get; }

  public bool AutoReuse { get; }

  public bool UseTurn { get; }

  public bool NoUseGraphic { get; }

  public bool NoMelee { get; }

  public bool NoWet { get; }

  public bool ShootsEveryUse { get; }

  public int ReuseDelay { get; }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
