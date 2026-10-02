using System.Collections.Frozen;

namespace Terraria.Content;

public sealed class ContentPresentationIndex
{
  public ContentPresentationIndex(
    ItemDefinitionCatalog items,
    NpcDefinitionCatalog npcs,
    ProjectileDefinitionCatalog projectiles)
  {
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(projectiles);
    ItemCreativeSortingByType = items.Definitions
      .Where(static definition => definition.Presentation.CreativeSorting.HasValue)
      .ToFrozenDictionary(
      definition => definition.Identity.TypeId,
      definition => definition.Presentation.CreativeSorting!.Value);
    NpcBestiarySortingIdByNetId = npcs.Definitions
      .Where(static definition => definition.Presentation.BestiarySortingId.HasValue)
      .ToFrozenDictionary(
        definition => definition.NetId,
        definition => definition.Presentation.BestiarySortingId!.Value);
    NpcBestiaryRarityStarsByType = npcs.Definitions
      .Where(static definition => definition.Presentation.BestiaryRarityStars.HasValue)
      .GroupBy(static definition => definition.TypeId)
      .ToFrozenDictionary(
        group => group.Key,
        group => group.Max(static definition => definition.Presentation.BestiaryRarityStars!.Value));
    DyeShaderIdByItemType = items.Definitions
      .Where(static definition => definition.Presentation.DyeShaderId.HasValue)
      .ToFrozenDictionary(
        definition => definition.Identity.TypeId,
        definition => definition.Presentation.DyeShaderId!.Value);
    ItemAnimationByType = items.Definitions
      .Where(static definition => definition.Presentation.Animation is not null)
      .ToFrozenDictionary(
        definition => definition.Identity.TypeId,
        definition => definition.Presentation.Animation!);
    ProjectileGlowMaskIdByType = projectiles.Definitions
      .Where(static definition => definition.Presentation.GlowMaskId.HasValue)
      .ToFrozenDictionary(
        definition => definition.TypeId,
        definition => definition.Presentation.GlowMaskId!.Value);
  }

  public FrozenDictionary<int, int> NpcBestiarySortingIdByNetId { get; }

  public FrozenDictionary<int, int> NpcBestiaryRarityStarsByType { get; }

  public FrozenDictionary<int, CreativeItemSortValue> ItemCreativeSortingByType { get; }

  public FrozenDictionary<int, int> DyeShaderIdByItemType { get; }

  public FrozenDictionary<int, AnimationDefinition> ItemAnimationByType { get; }

  public FrozenDictionary<int, short> ProjectileGlowMaskIdByType { get; }
}
