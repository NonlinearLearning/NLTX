using System.Collections.Generic;

namespace Terraria.Inventory;

/// <summary>Tick-scoped queue of world items offered to player inventories.</summary>
/// <remarks>
/// The queue lives on a dedicated world-scoped entity so the World tick can raise pickup intents
/// without receiving an inventory service, and the Inventory domain drains them at its own step.
/// </remarks>
public sealed class InventoryPickupRequestQueueComponent
{
  private readonly List<InventoryPickupRequest> _pending = [];

  public int Count => _pending.Count;

  public void Enqueue(in InventoryPickupRequest request) => _pending.Add(request);

  public IReadOnlyList<InventoryPickupRequest> Drain()
  {
    if (_pending.Count == 0)
    {
      return [];
    }

    InventoryPickupRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return requests;
  }

  public void Clear() => _pending.Clear();
}
