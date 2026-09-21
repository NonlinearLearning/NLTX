namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class ItemCommerceDefinition
{
  public ItemCommerceDefinition(
    bool isShopItem,
    bool buyOnce,
    int baseValue,
    bool canBuy,
    int shopSpecialCurrency = -1,
    int? shopCustomPrice = null)
  {
    if (baseValue < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(baseValue));
    }

    if (shopSpecialCurrency < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(shopSpecialCurrency));
    }

    if (shopCustomPrice is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(shopCustomPrice));
    }

    IsShopItem = isShopItem;
    BuyOnce = buyOnce;
    BaseValue = baseValue;
    CanBuy = canBuy;
    ShopSpecialCurrency = shopSpecialCurrency;
    ShopCustomPrice = shopCustomPrice;
  }

  public bool IsShopItem { get; }

  public bool BuyOnce { get; }

  public int BaseValue { get; }

  public bool CanBuy { get; }

  public int ShopSpecialCurrency { get; }

  public int? ShopCustomPrice { get; }
}
