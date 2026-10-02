using Terraria.Content.Items;

namespace Terraria.Combat.Items;

public static class ItemCapabilityProjection
{
  public readonly record struct View(
    ItemContentId ContentId,
    int RarityTier,
    ItemDamageClassCapability DamageClass);

  public static View Create(ItemContentId contentId, ItemCapabilitySnapshot capability)
  {
    ArgumentNullException.ThrowIfNull(capability);
    return new View(
      contentId,
      capability.Presentation.RarityTier,
      capability.DamageClass);
  }
}
