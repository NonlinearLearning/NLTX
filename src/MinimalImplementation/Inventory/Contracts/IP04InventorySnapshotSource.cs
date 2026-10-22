using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Contracts;

/// <summary>
/// Read-only snapshot boundary for owners that need to project P04 state.
/// It exposes no mutation or backing collection.
/// </summary>
public interface IP04InventorySnapshotSource
{
  bool TryCaptureInventory(
    EntityUuid playerId,
    out InventorySnapshot? snapshot);

  bool TryCaptureChest(
    WorldScopeId worldScope,
    ChestId chestId,
    out ChestSnapshot? snapshot);

  bool TryCaptureWorldItem(
    WorldItemHandle handle,
    out WorldItemSnapshot? snapshot);

  bool TryCaptureInstancedItem(
    EntityUuid playerId,
    WorldScopeId worldScope,
    int itemIndex,
    out InstancedItemSnapshot? snapshot);
}
