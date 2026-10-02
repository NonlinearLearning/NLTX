namespace Terraria.Player;

public sealed class PlayerCommerceCapabilityComponent
{
  public bool DiscountEquipped { get; internal set; }

  public bool DiscountAvailable { get; internal set; }

  public bool HasLuckyCoin { get; internal set; }

  public bool GoldRing { get; internal set; }

  internal void ResetEffects()
  {
    DiscountEquipped = false;
    DiscountAvailable = false;
    HasLuckyCoin = false;
    GoldRing = false;
  }
}
