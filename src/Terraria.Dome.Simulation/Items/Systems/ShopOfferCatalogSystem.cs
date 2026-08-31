using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Events;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ShopOfferCatalogSystem
{
  private readonly IDictionary<int, ShopOfferDefinition> _offers;

  public ShopOfferCatalogSystem(IDictionary<int, ShopOfferDefinition> offers)
  {
    ArgumentNullException.ThrowIfNull(offers);
    _offers = offers;
  }

  public bool ReplaceGeneratedOffers(IReadOnlyCollection<ShopOfferDefinition> generatedOffers)
  {
    ArgumentNullException.ThrowIfNull(generatedOffers);
    List<ShopOfferDefinition> validatedOffers = new(generatedOffers.Count);
    HashSet<int> offerIds = new();
    foreach (ShopOfferDefinition offer in generatedOffers)
    {
      try
      {
        offer.Validate();
      }
      catch (ArgumentOutOfRangeException)
      {
        return false;
      }

      if (!offerIds.Add(offer.OfferId))
      {
        return false;
      }

      validatedOffers.Add(offer);
    }

    _offers.Clear();
    for (int index = 0; index < validatedOffers.Count; index++)
    {
      ShopOfferDefinition offer = validatedOffers[index];
      _offers.Add(offer.OfferId, offer);
    }

    return true;
  }

  public bool ApplyPurchaseReceipt(ShopPurchaseReceipt receipt)
  {
    if (!_offers.TryGetValue(receipt.OfferId, out ShopOfferDefinition currentOffer))
    {
      return false;
    }

    try
    {
      receipt.OfferAfterCleanup.Validate();
    }
    catch (ArgumentOutOfRangeException)
    {
      return false;
    }

    if (!receipt.ResetCustomPrice && !receipt.ResetSpecialCurrency)
    {
      return currentOffer == receipt.OfferAfterCleanup;
    }

    if (!receipt.ResetCustomPrice || !receipt.ResetSpecialCurrency ||
        currentOffer.CustomPriceCopper is null ||
        receipt.OfferAfterCleanup.OfferId != receipt.OfferId ||
        receipt.OfferAfterCleanup.ItemType != currentOffer.ItemType ||
        receipt.OfferAfterCleanup.Quantity != currentOffer.Quantity ||
        receipt.OfferAfterCleanup.PriceCopper != currentOffer.PriceCopper ||
        receipt.OfferAfterCleanup.BuyOnce != currentOffer.BuyOnce ||
        receipt.OfferAfterCleanup.CustomPriceCopper is not null ||
        receipt.OfferAfterCleanup.SpecialCurrencyId != -1)
    {
      return false;
    }

    _offers[receipt.OfferId] = receipt.OfferAfterCleanup;
    return true;
  }
}
