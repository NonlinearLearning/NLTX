using Terraria.Items;

namespace Terraria.Items.Commerce;

public readonly record struct CommerceOffer(
  ExternalContentId OfferId,
  ItemDefinitionRef ItemDefinition,
  ExternalContentId VariantId,
  int AvailableQuantity,
  int MaximumQuantity,
  long BasePrice,
  ExternalContentId CurrencyId,
  long Revision);
