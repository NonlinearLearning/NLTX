namespace Terraria.WorldStorage;

public sealed class WorldStorageRoot
{
  private const int MaxProjectileSlots = 1000;

  public TileMapStore TileMap { get; } = new();
  public EntitySlotStore<WorldEntityState, PlayerSlot> Players { get; } = new(
    static slot => slot.Value,
    static value => new PlayerSlot(value));
  public EntitySlotStore<WorldEntityState, NpcSlot> Npcs { get; } = new(
    static slot => slot.Value,
    static value => new NpcSlot(value));
  public EntitySlotStore<WorldEntityState, ProjectileSlot> Projectiles { get; } = new(
    static slot => slot.Value,
    static value => new ProjectileSlot(value),
    MaxProjectileSlots);
  public EntitySlotStore<WorldEntityState, WorldItemSlot> WorldItems { get; } = new(
    static slot => slot.Value,
    static value => new WorldItemSlot(value));
  public ProjectileIdentityIndex ProjectileIdentities { get; } = new();
  public WorldContainerStore WorldContainers { get; } = new();
  public WorldSignStore WorldSigns { get; } = new();
  public TileEntityStore TileEntities { get; } = new();
  public TileEntityUpdateSchedule TileEntityUpdates { get; } = new();
  public WorldSectionState Sections { get; } = new();
}
