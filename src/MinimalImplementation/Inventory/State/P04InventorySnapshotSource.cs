using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Contracts;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Composes the P04 stores into one read-only, scope-aware snapshot boundary.</summary>
public sealed class P04InventorySnapshotSource : IP04InventorySnapshotSource
{
  private readonly InventoryStore _inventories;
  private readonly ChestStore _chests;
  private readonly WorldItemStore _worldItems;
  private readonly InstancedItemStore _instancedItems;

  public P04InventorySnapshotSource(
    InventoryStore inventories,
    ChestStore chests,
    WorldItemStore worldItems,
    InstancedItemStore instancedItems)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(worldItems);
    ArgumentNullException.ThrowIfNull(instancedItems);
    _inventories = inventories;
    _chests = chests;
    _worldItems = worldItems;
    _instancedItems = instancedItems;
  }

  public bool TryCaptureInventory(
    EntityUuid playerId,
    out InventorySnapshot? snapshot)
  {
    return _inventories.TryCapture(playerId, out snapshot);
  }

  public bool TryCaptureChest(
    WorldScopeId worldScope,
    ChestId chestId,
    out ChestSnapshot? snapshot)
  {
    if (!worldScope.IsValid
      || !_chests.TryCapture(chestId, out snapshot)
      || snapshot is null
      || snapshot.WorldScope != worldScope)
    {
      snapshot = null;
      return false;
    }

    return true;
  }

  public bool TryCaptureWorldItem(
    WorldItemHandle handle,
    out WorldItemSnapshot? snapshot)
  {
    if (!handle.IsValid
      || !_worldItems.TryCapture(handle.SlotId, out snapshot)
      || snapshot is null
      || snapshot.Handle != handle)
    {
      snapshot = null;
      return false;
    }

    return true;
  }

  public bool TryCaptureInstancedItem(
    EntityUuid playerId,
    WorldScopeId worldScope,
    int itemIndex,
    out InstancedItemSnapshot? snapshot)
  {
    if (!playerId.IsValid
      || !worldScope.IsValid
      || !_instancedItems.TryCapture(playerId, itemIndex, out snapshot)
      || snapshot is null
      || snapshot.WorldScope != worldScope)
    {
      snapshot = null;
      return false;
    }

    return true;
  }
}
