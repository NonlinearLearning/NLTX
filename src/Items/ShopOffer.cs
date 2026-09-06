namespace Terraria.Items;

public readonly record struct ShopOffer(
  int OfferId,
  int ItemContentId,
  int VariantId,
  int AvailableQuantity,
  int MaximumQuantity,
  long BasePrice,
  int CurrencyId);
