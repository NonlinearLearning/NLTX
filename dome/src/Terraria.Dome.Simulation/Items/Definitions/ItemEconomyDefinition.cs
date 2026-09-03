namespace Terraria.Dome.Simulation.Items.Definitions;

public readonly record struct ItemEconomyDefinition(
  bool IsShopItem = false,
  bool Buy = false,
  bool BuyOnce = false,
  int? CustomPriceCopper = null,
  int SpecialCurrencyId = -1);
