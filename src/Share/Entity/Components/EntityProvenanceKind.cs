namespace EntityEcs.Components;

public enum EntityProvenanceKind : byte
{
  Unknown,
  Parent,
  ItemUse,
  ItemUseWithAmmo,
  Projectile,
  OnHit,
  Mount,
  TileInteraction,
  TileBreak,
  Wiring,
  WorldEvent,
  WorldGeneration,
  Loot,
  SpawnNpc,
  Sync,
  RevengeSystem,
  FishedOut,
  DropAsItem,
  OverfullChest,
  DebugCommand,
  CoinRain
}
