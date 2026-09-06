namespace Terraria.Content;

public sealed record ItemEconomyDefinition(
  int Value,
  int? ShopCustomPrice,
  bool CanBuy = true,
  bool BuyOnce = false,
  int? ShopSpecialCurrencyId = null);
