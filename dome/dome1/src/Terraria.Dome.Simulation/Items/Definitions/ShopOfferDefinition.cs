using System;

namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ShopOfferDefinition(
  int OfferId,
  ushort ItemType,
  int Quantity,
  int PriceCopper,
  bool BuyOnce = false,
  int? CustomPriceCopper = null,
  int SpecialCurrencyId = -1)
{
  public bool UsesSpecialCurrency => SpecialCurrencyId >= 0;

  public int EffectivePrice => CustomPriceCopper ?? PriceCopper;

  public int EffectivePriceCopper => EffectivePrice;

  public static ShopOfferDefinition FromItemDefinition(
    int offerId,
    ItemDefinition definition,
    int quantity,
    int priceCopper)
  {
    if (definition.Economy is not ItemEconomyDefinition economy || !economy.IsShopItem || !economy.Buy)
    {
      throw new ArgumentException("The item is not a buyable shop item.", nameof(definition));
    }

    return new ShopOfferDefinition(
      offerId,
      definition.ItemType,
      quantity,
      priceCopper,
      economy.BuyOnce,
      economy.CustomPriceCopper,
      economy.SpecialCurrencyId);
  }

  public void Validate()
  {
    if (OfferId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(OfferId));
    }

    if (ItemType == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ItemType));
    }

    if (Quantity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(Quantity));
    }

    if (PriceCopper < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(PriceCopper));
    }

    if (CustomPriceCopper is int customPriceCopper && customPriceCopper < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(CustomPriceCopper));
    }

    if (SpecialCurrencyId < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(SpecialCurrencyId));
    }
  }
}
