using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Prepares the original two equipment swaps for the P02 packet 147 transaction.</summary>
public sealed class InventoryLoadoutCoordinator
{
  private readonly InventoryStore _inventories;
  private readonly WorldItemProjectionJournal _projections;

  public InventoryLoadoutCoordinator(InventoryStore inventories, WorldItemStore items)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(items);
    if (!ReferenceEquals(inventories.CommitGate, items.CommitGate))
    {
      throw new ArgumentException("Loadouts and their projections require the inventory gate.");
    }

    _inventories = inventories;
    _projections = items.Projections;
  }

  public IPlayerLoadoutTransaction Prepare(
    SessionBinding binding, Player.Player player, SyncLoadoutPacket packet)
  {
    ArgumentNullException.ThrowIfNull(binding);
    ArgumentNullException.ThrowIfNull(player);
    ArgumentNullException.ThrowIfNull(packet);
    lock (_inventories.CommitGate.SyncRoot)
    {
      if (binding.PlayerHandle != player.Handle || binding.Epoch != player.Epoch
        || packet.Player != binding.PlayerSlot || !binding.Epoch.IsValid
        || !binding.PlayerHandle.WorldScope.IsValid
        || player.LoadoutIndex >= InventorySlotLayout.LoadoutCount
        || packet.LoadoutIndex >= InventorySlotLayout.LoadoutCount)
      {
        return new RejectedTransaction(OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "The loadout identity or index is invalid."));
      }

      if (!_inventories.TryCapture(player.Handle.Id, out InventorySnapshot? before)
        || before is null || before.Slots.Count < InventorySlotLayout.Count)
      {
        return new RejectedTransaction(new OwnerCommitResult(
          OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
          "The complete Version4 equipment storage is not available."));
      }

      var values = before.Slots.Select(slot => slot.Item).ToArray();
      if (player.LoadoutIndex != packet.LoadoutIndex)
      {
        Swap(values, player.LoadoutIndex);
        Swap(values, packet.LoadoutIndex);
      }

      var mutations = new List<InventoryMutation>();
      for (int index = 0; index < values.Length; index++)
      {
        if (values[index] != before.Slots[index].Item)
        {
          mutations.Add(new InventoryMutation(
            (short)index, before.Slots[index].Generation, values[index]));
        }
      }

      OwnerCommitResult result = _inventories.PrepareApply(
        before.PlayerId, before.Revision, mutations, out var prepared);
      return prepared is null
        ? new RejectedTransaction(result)
        : new Transaction(this, binding, player, player.LoadoutIndex, prepared);
    }
  }

  private static void Swap(InventoryItemState[] values, byte loadout)
  {
    int stored = InventorySlotLayout.LoadoutStart + loadout * InventorySlotLayout.LoadoutSize;
    for (int index = 0; index < InventorySlotLayout.LoadoutSize; index++)
    {
      int active = InventorySlotLayout.ArmorStart + index;
      (values[active], values[stored + index]) = (values[stored + index], values[active]);
    }
  }

  private sealed class RejectedTransaction(OwnerCommitResult result) : IPlayerLoadoutTransaction
  {
    public OwnerCommitResult Validate() => result;

    public void Commit()
    {
      throw new InvalidOperationException("A rejected loadout transaction cannot commit.");
    }
  }

  private sealed class Transaction(
    InventoryLoadoutCoordinator owner, SessionBinding binding, Player.Player player,
    byte previousLoadout, InventoryStore.PreparedMutation prepared) : IPlayerLoadoutTransaction
  {
    private bool _committed;

    public OwnerCommitResult Validate()
    {
      lock (owner._inventories.CommitGate.SyncRoot)
      {
        if (_committed || player.Handle != binding.PlayerHandle || player.Epoch != binding.Epoch
          || player.LoadoutIndex != previousLoadout || !owner._inventories.CanCommit(prepared))
        {
          return new OwnerCommitResult(OwnerCommitStatus.Conflict, null,
            ProtocolErrorCode.Conflict, "The prepared equipment or player identity changed.");
        }

        int inventoryTargets = prepared.HasChanges
          ? owner._projections.CountInventoryTargets(
            binding.PlayerHandle.WorldScope, binding.Epoch) : 0;
        return owner._projections.CanPublish([], [], inventoryTargets)
          ? OwnerCommitResult.Applied(prepared.NextRevision)
          : new OwnerCommitResult(OwnerCommitStatus.Unavailable, null,
            ProtocolErrorCode.NotReady, "The projection sequence space is exhausted.");
      }
    }

    public void Commit()
    {
      lock (owner._inventories.CommitGate.SyncRoot)
      {
        if (Validate().Status != OwnerCommitStatus.Applied)
        {
          throw new InvalidOperationException("Loadout validation must precede the exclusive commit.");
        }

        owner._inventories.CommitPrepared(prepared, out _);
        _committed = true;
        if (prepared.HasChanges)
        {
          owner._projections.PublishInventory(
            binding.PlayerHandle.WorldScope, binding.Epoch, prepared.Snapshot);
        }
      }
    }
  }
}
