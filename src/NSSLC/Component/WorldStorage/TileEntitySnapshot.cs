using Terraria.Items;

namespace Terraria.WorldStorage;

/// <summary>
/// Immutable persistence DTO for a TileEntity. Live, updateable fields are hydrated into the
/// corresponding type capability component and are projected back into a fresh snapshot on capture.
/// </summary>
public sealed record TileEntitySnapshot {
  public TileEntityId Id { get; }
  public TileEntityTypeId Type { get; }
  public TileCoordinate Anchor { get; }
  public IReadOnlyList<ItemState> Items { get; }
  public short NpcIndex { get; }
  public byte LogicCheck { get; }
  public bool LogicOn { get; }
  public byte Pose { get; }

  public TileEntitySnapshot(TileEntityId id, TileEntityTypeId type, TileCoordinate anchor,
      IReadOnlyList<ItemState> items, short npcIndex = -1, byte logicCheck = 0,
      bool logicOn = false, byte pose = 0) {
    Id = id;
    Type = type;
    Anchor = anchor;
    Items = Array.AsReadOnly(items.ToArray());
    NpcIndex = npcIndex;
    LogicCheck = logicCheck;
    LogicOn = logicOn;
    Pose = pose;
  }
}

/// <summary>An immutable capture of tile entity persistence state and its next identity.</summary>
public sealed record TileEntityStoreSnapshot {
  public IReadOnlyList<TileEntitySnapshot> Entities { get; }
  public int NextId { get; }

  public TileEntityStoreSnapshot(IReadOnlyList<TileEntitySnapshot> entities, int nextId) {
    ArgumentNullException.ThrowIfNull(entities);
    ArgumentOutOfRangeException.ThrowIfNegative(nextId);
    var ids = new HashSet<int>();
    var anchors = new HashSet<TileCoordinate>();
    foreach (TileEntitySnapshot entity in entities) {
      ArgumentNullException.ThrowIfNull(entity);
      if (entity.Id.Value < 0 || entity.Id.Value >= nextId || !ids.Add(entity.Id.Value)) {
        throw new InvalidDataException("TileEntity snapshot IDs are invalid or duplicated.");
      }
      if (!anchors.Add(entity.Anchor)) {
        throw new InvalidDataException("TileEntity snapshot anchors are duplicated.");
      }
    }

    Entities = Array.AsReadOnly(entities.ToArray());
    NextId = nextId;
  }
}
