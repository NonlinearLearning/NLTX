namespace Terraria.EntityLifecycleAttribution;

public readonly record struct LegacyEntitySourceRecord(
  EntitySpawnSourceKind Kind,
  EntityReference? PrimaryEntity,
  EntityReference? SecondaryEntity,
  TileCoordinate? TileCoords,
  int? SourceId,
  int? AmmoItemIdUsed,
  int? MountId)
{
  public static LegacyEntitySourceRecord OnHit(EntityReference striking, EntityReference struck)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.OnHit,
      striking,
      struck,
      TileCoords: null,
      SourceId: null,
      AmmoItemIdUsed: null,
      MountId: null);
  }

  public static LegacyEntitySourceRecord Tile(TileCoordinate tileCoords)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.Tile,
      PrimaryEntity: null,
      SecondaryEntity: null,
      tileCoords,
      SourceId: null,
      AmmoItemIdUsed: null,
      MountId: null);
  }

  public static LegacyEntitySourceRecord ItemSourceId(EntityReference entity, int sourceId)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.ItemSourceId,
      entity,
      SecondaryEntity: null,
      TileCoords: null,
      sourceId,
      AmmoItemIdUsed: null,
      MountId: null);
  }

  public static LegacyEntitySourceRecord ProjectileSourceId(int sourceId)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.ProjectileSourceId,
      PrimaryEntity: null,
      SecondaryEntity: null,
      TileCoords: null,
      sourceId,
      AmmoItemIdUsed: null,
      MountId: null);
  }

  public static LegacyEntitySourceRecord ItemUse(EntityReference entity)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.ItemUse,
      entity,
      SecondaryEntity: null,
      TileCoords: null,
      SourceId: null,
      AmmoItemIdUsed: null,
      MountId: null);
  }

  public static LegacyEntitySourceRecord ItemUseWithAmmo(EntityReference entity, int ammoItemIdUsed)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.ItemUseWithAmmo,
      entity,
      SecondaryEntity: null,
      TileCoords: null,
      SourceId: null,
      ammoItemIdUsed,
      MountId: null);
  }

  public static LegacyEntitySourceRecord Mount(EntityReference entity, int mountId)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.Mount,
      entity,
      SecondaryEntity: null,
      TileCoords: null,
      SourceId: null,
      AmmoItemIdUsed: null,
      mountId);
  }

  public static LegacyEntitySourceRecord OverfullChest(int chestSlot)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.OverfullChest,
      PrimaryEntity: null,
      SecondaryEntity: null,
      TileCoords: new TileCoordinate(chestSlot, 0),
      SourceId: null,
      AmmoItemIdUsed: null,
      MountId: null);
  }

  public static LegacyEntitySourceRecord Parent(EntityReference parent)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.Parent,
      parent,
      SecondaryEntity: null,
      TileCoords: null,
      SourceId: null,
      AmmoItemIdUsed: null,
      MountId: null);
  }

  public static LegacyEntitySourceRecord TileInteraction(EntityReference entity)
  {
    return new LegacyEntitySourceRecord(
      EntitySpawnSourceKind.TileInteraction,
      entity,
      SecondaryEntity: null,
      TileCoords: null,
      SourceId: null,
      AmmoItemIdUsed: null,
      MountId: null);
  }
}
