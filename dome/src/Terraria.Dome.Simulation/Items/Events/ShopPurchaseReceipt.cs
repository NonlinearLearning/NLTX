using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Events;

public readonly record struct ShopPurchaseReceipt(
  PlayerHandle Player,
  NpcHandle Npc,
  int OfferId,
  long Sequence,
  bool ResetCustomPrice,
  bool ResetSpecialCurrency,
  ShopOfferDefinition OfferAfterCleanup)
{
  public static ShopPurchaseReceipt Create(
    ShopPurchaseCommand command,
    ShopOfferDefinition offer)
  {
    bool resetTransientPrice = offer.CustomPriceCopper.HasValue;
    ShopOfferDefinition offerAfterCleanup = resetTransientPrice
      ? offer with
      {
        CustomPriceCopper = null,
        SpecialCurrencyId = -1
      }
      : offer;
    return new ShopPurchaseReceipt(
      command.Player,
      command.Npc,
      command.OfferId,
      command.Sequence,
      resetTransientPrice,
      resetTransientPrice,
      offerAfterCleanup);
  }
}
