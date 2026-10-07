using System;
using EntityEcs;

namespace Terraria.WorldStorage;

public sealed class WorldStorageRoot : IDisposable
{
  private const int MaxProjectileSlots = 1000;
  private bool _isDisposed;

  public WorldStorageRoot(
    EntityRuntime entityRuntime,
    int maximumWorldItemSlots = int.MaxValue)
  {
    EntityRuntime = entityRuntime ?? throw new ArgumentNullException(nameof(entityRuntime));
    Players = new EntitySlotStore<WorldEntityState, PlayerSlot>(
      static slot => slot.Value,
      static value => new PlayerSlot(value));
    Npcs = new EntitySlotStore<WorldEntityState, NpcSlot>(
      static slot => slot.Value,
      static value => new NpcSlot(value));
    Projectiles = new EntitySlotStore<WorldEntityState, ProjectileSlot>(
      static slot => slot.Value,
      static value => new ProjectileSlot(value),
      MaxProjectileSlots);
    WorldItems = new EntitySlotStore<WorldEntityState, WorldItemSlot>(
      static slot => slot.Value,
      static value => new WorldItemSlot(value),
      maximumWorldItemSlots);
    TileEntityUpdates = new TileEntityUpdateSchedule();
    TileEntities = new TileEntityStore(EntityRuntime, TileEntityUpdates);
  }

  public EntityRuntime EntityRuntime { get; }

  // Compatibility aliases; each resolves to the session's one authoritative runtime.
  public EntityRuntime ProjectileRuntime => EntityRuntime;

  public EntityRuntime TileEntityRuntime => EntityRuntime;

  public TileMapStore TileMap { get; } = new();
  public EntitySlotStore<WorldEntityState, PlayerSlot> Players { get; }
  public EntitySlotStore<WorldEntityState, NpcSlot> Npcs { get; }
  public EntitySlotStore<WorldEntityState, ProjectileSlot> Projectiles { get; }
  public EntitySlotStore<WorldEntityState, WorldItemSlot> WorldItems { get; }
  public ProjectileIdentityIndex ProjectileIdentities { get; } = new();
  public WorldContainerStore WorldContainers { get; } = new();
  public WorldSignStore WorldSigns { get; } = new();
  public TileEntityUpdateSchedule TileEntityUpdates { get; }
  public TileEntityStore TileEntities { get; }
  public WorldSectionState Sections { get; } = new();
  public WorldPressurePlateRegistryComponent PressurePlates { get; } = new();

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    TileEntities.Dispose();
    Players.Dispose();
    Npcs.Dispose();
    Projectiles.Dispose();
    WorldItems.Dispose();
    ProjectileIdentities.Dispose();
    WorldContainers.Replace(Array.Empty<WorldChestSnapshot>());
    WorldSigns.Replace(Array.Empty<WorldSignSnapshot>());
    TileMap.Dispose();
    TileEntityUpdates.Dispose();
    PressurePlates.Dispose();
    _isDisposed = true;
  }
}
