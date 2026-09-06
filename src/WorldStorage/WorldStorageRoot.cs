namespace Terraria.WorldStorage;

public sealed class WorldStorageRoot
{
  public TileMapStore TileMap { get; } = new();
  public EntitySlotStore<WorldEntityState, PlayerSlot> Players { get; } = new();
  public EntitySlotStore<WorldEntityState, NpcSlot> Npcs { get; } = new();
  public EntitySlotStore<WorldEntityState, ProjectileSlot> Projectiles { get; } = new();
  public EntitySlotStore<WorldEntityState, WorldItemSlot> WorldItems { get; } = new();
  public ProjectileIdentityIndex ProjectileIdentities { get; } = new();
  public WorldContainerStore WorldContainers { get; } = new();
  public WorldSignStore WorldSigns { get; } = new();
  public TileEntityStore TileEntities { get; } = new();
  public TileEntityUpdateSchedule TileEntityUpdates { get; } = new();
  public WorldSectionState Sections { get; } = new();
}
