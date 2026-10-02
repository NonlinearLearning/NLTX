namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingSchedulerState
{
  private readonly List<EmergencyStackingTransferPlan> _pendingTransfers = new();
  private readonly HashSet<string> _pendingItemKeys = new();

  public IReadOnlyList<EmergencyStackingTransferPlan> PendingTransfers =>
    _pendingTransfers.AsReadOnly();

  public bool HasPendingTransfer(string itemKey)
  {
    return _pendingItemKeys.Contains(itemKey);
  }

  public bool TryAdd(EmergencyStackingTransferPlan transfer)
  {
    ArgumentNullException.ThrowIfNull(transfer);
    if (_pendingItemKeys.Contains(transfer.Source.ItemKey)
      || _pendingItemKeys.Contains(transfer.Destination.ItemKey))
    {
      return false;
    }

    _pendingTransfers.Add(transfer);
    _pendingItemKeys.Add(transfer.Source.ItemKey);
    _pendingItemKeys.Add(transfer.Destination.ItemKey);
    return true;
  }

  public bool ClearForItem(string itemKey)
  {
    if (string.IsNullOrWhiteSpace(itemKey))
    {
      return false;
    }

    int removed = _pendingTransfers.RemoveAll(
      transfer => transfer.Source.ItemKey == itemKey || transfer.Destination.ItemKey == itemKey);
    if (removed == 0)
    {
      return false;
    }

    _pendingItemKeys.Clear();
    foreach (EmergencyStackingTransferPlan pendingTransfer in _pendingTransfers)
    {
      _pendingItemKeys.Add(pendingTransfer.Source.ItemKey);
      _pendingItemKeys.Add(pendingTransfer.Destination.ItemKey);
    }

    return true;
  }

  public void Clear()
  {
    _pendingTransfers.Clear();
    _pendingItemKeys.Clear();
  }
}
