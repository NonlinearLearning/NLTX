namespace Terraria.EntityLifecycleAttribution;

public enum EntitySpawnSourceKind
{
  Unknown = 0,
  OnHit = 1,
  Tile = 2,
  ItemSourceId = 3,
  ProjectileSourceId = 4,
  ItemUse = 5,
  ItemUseWithAmmo = 6,
  Mount = 7,
  OverfullChest = 8,
  Parent = 9,
  TileInteraction = 10,
}
