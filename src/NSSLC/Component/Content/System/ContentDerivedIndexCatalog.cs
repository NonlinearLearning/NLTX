using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Terraria.Content;

public sealed class ContentDerivedIndexCatalog
{
  public ContentDerivedIndexCatalog(
    ItemDefinitionCatalog items,
    ProjectileDefinitionCatalog projectiles,
    NpcDefinitionCatalog npcs)
  {
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(projectiles);
    ArgumentNullException.ThrowIfNull(npcs);
    ProjectileHostileByType = projectiles.Definitions.ToFrozenDictionary(
      definition => definition.TypeId,
      definition => definition.Combat.Hostile);
    ProjectileArrowByType = projectiles.Definitions.ToFrozenDictionary(
      definition => definition.TypeId,
      definition => definition.Capabilities.IsArrow);
    ProjectileHookByType = projectiles.Definitions.ToFrozenDictionary(
      definition => definition.TypeId,
      definition => definition.Capabilities.IsHook);
    NpcNetIdsByType = npcs.Definitions
      .GroupBy(definition => definition.TypeId)
      .ToFrozenDictionary(
        group => group.Key,
        group => group.Select(definition => definition.NetId).ToImmutableArray());
    ItemTypesByHeadSlot = BuildEquipmentSlotIndex(items.Definitions, static definition => definition.Equipment.HeadSlot);
    ItemTypesByBodySlot = BuildEquipmentSlotIndex(items.Definitions, static definition => definition.Equipment.BodySlot);
    ItemTypesByLegSlot = BuildEquipmentSlotIndex(items.Definitions, static definition => definition.Equipment.LegSlot);
    ItemTypesByCreatedWall = BuildPlacementIndex(
      items.Definitions,
      static definition => definition.Placement.CreateWallTypeId);
    ItemTypesByCreatedTile = BuildPlacementIndex(
      items.Definitions,
      static definition => definition.Placement.CreateTileTypeId);
  }

  public FrozenDictionary<int, bool> ProjectileHostileByType { get; }

  public FrozenDictionary<int, bool> ProjectileArrowByType { get; }

  public FrozenDictionary<int, bool> ProjectileHookByType { get; }

  public FrozenDictionary<int, ImmutableArray<int>> NpcNetIdsByType { get; }

  public FrozenDictionary<int, int> ItemTypesByHeadSlot { get; }

  public FrozenDictionary<int, int> ItemTypesByBodySlot { get; }

  public FrozenDictionary<int, int> ItemTypesByLegSlot { get; }

  public FrozenDictionary<int, ImmutableArray<int>> ItemTypesByCreatedWall { get; }

  public FrozenDictionary<int, ImmutableArray<int>> ItemTypesByCreatedTile { get; }

  private static FrozenDictionary<int, int> BuildEquipmentSlotIndex(
    IEnumerable<ItemDefinition> definitions,
    Func<ItemDefinition, int?> slotSelector)
  {
    return definitions
      .Where(definition => slotSelector(definition).HasValue)
      .ToFrozenDictionary(
        definition => slotSelector(definition)!.Value,
        definition => definition.Identity.TypeId);
  }

  private static FrozenDictionary<int, ImmutableArray<int>> BuildPlacementIndex(
    IEnumerable<ItemDefinition> definitions,
    Func<ItemDefinition, int?> typeIdSelector)
  {
    return definitions
      .Where(definition => typeIdSelector(definition).HasValue)
      .GroupBy(definition => typeIdSelector(definition)!.Value)
      .ToFrozenDictionary(
        group => group.Key,
        group => group.Select(definition => definition.Identity.TypeId).ToImmutableArray());
  }
}
