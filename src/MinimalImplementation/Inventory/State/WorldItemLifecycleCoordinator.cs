using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Retires session/world-owned state under the common Inventory commit gate.</summary>
public sealed class WorldItemLifecycleCoordinator
{
  private readonly WorldItemStore _items;
  private readonly WorldItemReservationStore _claims;
  private readonly InstancedItemStore _instanced;
  private readonly ChestLeaseStore _leases;
  private readonly WorldItemTransactionCoordinator _transactions;

  public WorldItemLifecycleCoordinator(
    InventoryStore inventories, WorldItemStore items, WorldItemReservationStore claims,
    InstancedItemStore instanced, ChestLeaseStore leases)
  {
    ArgumentNullException.ThrowIfNull(instanced);
    ArgumentNullException.ThrowIfNull(leases);
    _transactions = new WorldItemTransactionCoordinator(inventories, items, claims);
    if (!ReferenceEquals(items.CommitGate, instanced.CommitGate)
      || !ReferenceEquals(items.CommitGate, leases.CommitGate))
    {
      throw new ArgumentException("Inventory lifecycle owners must share one commit gate.");
    }

    _items = items;
    _claims = claims;
    _instanced = instanced;
    _leases = leases;
  }

  /// <summary>
  /// Call on ServerLoop for the exact closing epoch. Persistent player inventory stays owned
  /// by InventoryStore; a close does not delete the reconnecting player's saved inventory.
  /// </summary>
  public OwnerCommitResult ReleaseSession(SessionEpoch epoch)
  {
    if (!epoch.IsValid)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "Invalid closing epoch.");
    }

    lock (_items.CommitGate.SyncRoot)
    {
      WorldItemHandle[] handles = _claims.CaptureSession(epoch)
        .Select(claim => claim.Handle).ToArray();
      OwnerCommitResult result = _transactions.Prepare(
        [], [], handles, null, [], out var prepared);
      if (prepared is null)
      {
        return result;
      }

      result = prepared.Commit().Result;
      if (result.Status == OwnerCommitStatus.Applied)
      {
        _instanced.ReleaseSession(epoch);
        _leases.ReleaseSession(epoch);
        _items.Projections.ReleaseSession(epoch);
      }

      return result;
    }
  }

  /// <summary>Host calls after stopping the old world's ticks and retiring its audience.</summary>
  public OwnerCommitResult RetireWorldScope(WorldScopeId scope)
  {
    if (!scope.IsValid)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "Invalid retiring world.");
    }

    lock (_items.CommitGate.SyncRoot)
    {
      WorldItemChange[] changes = _items.CaptureWorld(scope)
        .Select(item => new WorldItemChange(item.Handle, item.Revision, null)).ToArray();
      WorldItemHandle[] handles = _claims.CaptureWorld(scope)
        .Select(claim => claim.Handle).ToArray();
      OwnerCommitResult result = _transactions.Prepare(
        changes, [], handles, null, [], out var prepared);
      if (prepared is null)
      {
        return result;
      }

      result = prepared.Commit().Result;
      if (result.Status == OwnerCommitStatus.Applied)
      {
        _instanced.ReleaseWorldScope(scope);
        _leases.ReleaseWorldScope(scope);
        _items.Projections.ReleaseWorldScope(scope);
      }

      return result;
    }
  }
}
