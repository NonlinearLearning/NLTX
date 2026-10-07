using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.Items;
using Terraria.WorldStorage;
using RuntimeTileEntities = NSSLC.WorldGeneration.GameContent.Tile_Entities;

namespace NSSLC.WorldGeneration;

/// <summary>In-memory identities of generated objects with independent contents.</summary>
public class TileEntity {
  private static Dictionary<(int X, int Y), TileEntity> _entities = new();
  private static int _nextId;
  public int Id { get; private set; }
  public int X { get; private set; }
  public int Y { get; private set; }
  public int TileType { get; private set; }

  protected TileEntity() {
  }

  private TileEntity(int id, int x, int y, int type) {
    Initialize(this, id, x, y, type);
  }

  public static int Register(int x, int y, int type) {
    if (_entities.TryGetValue((x, y), out TileEntity existing)) {
      return existing.Id;
    }
    var entity = new TileEntity(_nextId, x, y, type);
    _entities.Add((x, y), entity);
    _nextId++;
    return entity.Id;
  }

  public static int Register<T>(int x, int y, int type) where T : TileEntity, new() {
    bool hasExisting = _entities.TryGetValue((x, y), out TileEntity existing);
    if (hasExisting && existing is T typedExisting) {
      return typedExisting.Id;
    }

    var entity = new T();
    int id = hasExisting ? existing.Id : _nextId;
    Initialize(entity, id, x, y, type);
    _entities[(x, y)] = entity;
    if (!hasExisting) {
      _nextId++;
    }
    return entity.Id;
  }

  public static T CreateForRestore<T>(int id, int x, int y, int type)
      where T : TileEntity, new() {
    if (id < 0) {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    var entity = new T();
    Initialize(entity, id, x, y, type);
    return entity;
  }

  public static void ReplaceLoaded(IReadOnlyList<TileEntity> entities, int nextId) {
    ArgumentNullException.ThrowIfNull(entities);
    if (nextId < 0) {
      throw new ArgumentOutOfRangeException(nameof(nextId));
    }

    var byAnchor = new Dictionary<(int X, int Y), TileEntity>(entities.Count);
    var ids = new HashSet<int>();
    foreach (TileEntity entity in entities) {
      ArgumentNullException.ThrowIfNull(entity);
      if (entity.Id < 0 || entity.Id >= nextId || !ids.Add(entity.Id)) {
        throw new ArgumentException("Loaded TileEntity IDs are invalid or duplicated.",
            nameof(entities));
      }
      if (!byAnchor.TryAdd((entity.X, entity.Y), entity)) {
        throw new ArgumentException("Loaded TileEntity anchors are duplicated.",
            nameof(entities));
      }
    }

    _entities = byAnchor;
    _nextId = nextId;
  }

  /// <summary>Replaces a committed entity while preserving its indexed identity.</summary>
  public static bool ReplaceAtAnchor(TileEntity entity) {
    ArgumentNullException.ThrowIfNull(entity);
    if (!_entities.TryGetValue((entity.X, entity.Y), out TileEntity existing) ||
        existing.Id != entity.Id) {
      return false;
    }

    _entities[(entity.X, entity.Y)] = entity;
    return true;
  }

  /// <summary>Removes an entity only when its anchor and ID still match.</summary>
  public static bool RemoveAtAnchor(int x, int y, int id) {
    return _entities.TryGetValue((x, y), out TileEntity existing) &&
      existing.Id == id && _entities.Remove((x, y));
  }

  private static void Initialize(TileEntity entity, int id, int x, int y, int type) {
    entity.Id = id;
    entity.X = x;
    entity.Y = y;
    entity.TileType = type;
  }

  public static void Clear() {
    _entities.Clear();
    _nextId = 0;
  }

  internal static IReadOnlyList<GeneratedTileEntity> CreateSnapshot() {
    var result = new List<GeneratedTileEntity>(_entities.Count);
    foreach (TileEntity entity in _entities.Values) {
      result.Add(new GeneratedTileEntity(entity.Id, entity.X, entity.Y, entity.TileType));
    }
    return result.AsReadOnly();
  }

  /// <summary>Captures persistable entity data in stable ID order.</summary>
  public static TileEntityStoreSnapshot CreatePersistenceSnapshot() {
    var result = new List<TileEntitySnapshot>(_entities.Count);
    foreach (TileEntity entity in _entities.Values.OrderBy(static entity => entity.Id)) {
      result.Add(CapturePersistenceState(entity));
    }

    return new TileEntityStoreSnapshot(result, _nextId);
  }

  private static TileEntitySnapshot CapturePersistenceState(TileEntity entity) {
    IReadOnlyList<ItemState> items = Array.Empty<ItemState>();
    short npcIndex = -1;
    byte logicCheck = 0;
    bool logicOn = false;
    byte pose = 0;
    byte type;

    switch (entity) {
      case RuntimeTileEntities.TETrainingDummy trainingDummy:
        type = 0;
        npcIndex = checked((short)trainingDummy.npc);
        break;
      case RuntimeTileEntities.TEItemFrame itemFrame:
        type = 1;
        items = CaptureItems(itemFrame.item);
        break;
      case NSSLC.WorldGeneration.TELogicSensor logicSensor:
        type = 2;
        logicCheck = logicSensor.logicCheck;
        logicOn = logicSensor.On;
        break;
      case NSSLC.WorldGeneration.TEDisplayDoll displayDoll:
        type = 3;
        items = CaptureItems(displayDoll.items);
        pose = displayDoll.pose;
        break;
      case RuntimeTileEntities.TEWeaponsRack weaponsRack:
        type = 4;
        items = CaptureItems(weaponsRack.item);
        break;
      case NSSLC.WorldGeneration.TEWeaponsRack legacyWeaponsRack:
        type = 4;
        items = CaptureItems(legacyWeaponsRack.item);
        break;
      case NSSLC.WorldGeneration.TEHatRack hatRack:
        type = 5;
        items = CaptureItems(hatRack.items);
        break;
      case RuntimeTileEntities.TEFoodPlatter foodPlatter:
        type = 6;
        items = CaptureItems(foodPlatter.item);
        break;
      case NSSLC.WorldGeneration.TETeleportationPylon:
        type = 7;
        break;
      case RuntimeTileEntities.TEDeadCellsDisplayJar:
        type = 8;
        break;
      case NSSLC.WorldGeneration.TEKiteAnchor:
        type = 9;
        break;
      case NSSLC.WorldGeneration.TECritterAnchor:
        type = 10;
        break;
      default:
        throw new InvalidDataException(
          $"Runtime TileEntity {entity.GetType().FullName} has no persistence mapping.");
    }

    return new TileEntitySnapshot(
      new TileEntityId(entity.Id),
      new TileEntityTypeId(type),
      new TileCoordinate(entity.X, entity.Y),
      items,
      npcIndex,
      logicCheck,
      logicOn,
      pose);
  }

  private static IReadOnlyList<ItemState> CaptureItems(NSSLC.WorldGeneration.Item item) {
    ArgumentNullException.ThrowIfNull(item);
    return Array.AsReadOnly(new[] { CaptureItem(item) });
  }

  private static IReadOnlyList<ItemState> CaptureItems(NSSLC.WorldGeneration.Item[] items) {
    ArgumentNullException.ThrowIfNull(items);
    var result = new ItemState[items.Length];
    for (int index = 0; index < items.Length; index++) {
      result[index] = CaptureItem(items[index]);
    }

    return Array.AsReadOnly(result);
  }

  private static ItemState CaptureItem(NSSLC.WorldGeneration.Item item) {
    ArgumentNullException.ThrowIfNull(item);
    return new ItemState(item.type, item.prefix, item.stack);
  }

  public static void PerformUpdates() { }

  public virtual void OnWorldLoaded() { }

  public static bool TryGetAt<T>(int x, int y, out T entity) where T : class {
    entity = null;
    return _entities.TryGetValue((x, y), out TileEntity value) && (entity = value as T) != null;
  }

  public static void Write(params object[] arguments) {
    throw new NotSupportedException("TileEntity encoding belongs to the world storage adapter.");
  }

  public static TileEntity Read(params object[] arguments) {
    throw new NotSupportedException("TileEntity decoding belongs to the world storage adapter.");
  }
}
