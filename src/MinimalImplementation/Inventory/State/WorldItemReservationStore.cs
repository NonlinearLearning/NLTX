using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Claims share the item commit gate; reads never expire or mutate reservations.</summary>
public sealed class WorldItemReservationStore
{
  private readonly Dictionary<int, WorldItemReservation> _bySlot = new();
  private readonly Dictionary<WorldItemHandle, CommitVersion> _versions = new();
  private readonly WorldItemStore? _items;

  public WorldItemReservationStore() : this(new ServerStateCommitGate())
  {
  }

  public WorldItemReservationStore(ServerStateCommitGate commitGate)
  {
    ArgumentNullException.ThrowIfNull(commitGate);
    CommitGate = commitGate;
  }

  public WorldItemReservationStore(WorldItemStore items) : this(items.CommitGate)
  {
    _items = items;
    items.BindReservations(this);
  }

  internal ServerStateCommitGate CommitGate { get; }

  public ReservationResult TryReserve(
    WorldItemSnapshot item, EntityUuid playerId, SessionEpoch epoch,
    DateTimeOffset now, TimeSpan duration)
  {
    if (_items is not null)
    {
      return ReservationResult.Unavailable(
        "A published reservation requires a captured authenticated owner binding.");
    }

    return TryReserve(item, playerId, epoch, now, duration, null);
  }

  /// <summary>Host retains its exclusive ServerLoop window from capture through commit.</summary>
  public ReservationResult TryReserve(
    WorldItemSnapshot item, WorldItemOwnerBindingCapture owner,
    DateTimeOffset now, TimeSpan duration)
  {
    ArgumentNullException.ThrowIfNull(owner);
    if (_items is not null)
    {
      return ReservationResult.Unavailable(
        "Published ownership uses frame claims, not duration leases.");
    }
    return TryReserve(item, owner.Binding.PlayerHandle.Id, owner.Binding.Epoch,
      now, duration, owner);
  }

  public ReservationResult TryReserveFrame(
    WorldItemSnapshot item, WorldItemOwnerBindingCapture owner)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentNullException.ThrowIfNull(owner);
    lock (CommitGate.SyncRoot)
    {
      WorldItemReservation? before = _bySlot.GetValueOrDefault(item.Handle.SlotId);
      if (before is not null && (before.Handle != item.Handle
        || before.PlayerId != owner.Binding.PlayerHandle.Id || before.Epoch != owner.Binding.Epoch))
      {
        return ReservationResult.Conflict("The item is owned by another captured session.");
      }
      var change = new WorldItemOwnershipChange(item, before, owner,
        before is { FrameBased: true } ? before.ReleaseCounter : 0, true);
      if (_items is null || !PrepareChanges([], [change], out var prepared)
        || !_items.Projections.CanPublish([], [item.Handle], 0))
      {
        return ReservationResult.Unavailable("The frame ownership change cannot be prepared.");
      }
      CommitPrepared(prepared);
      return ReservationResult.Applied(prepared.Entries[0].After!);
    }
  }

  private ReservationResult TryReserve(
    WorldItemSnapshot item, EntityUuid playerId, SessionEpoch epoch,
    DateTimeOffset now, TimeSpan duration, WorldItemOwnerBindingCapture? owner)
  {
    ArgumentNullException.ThrowIfNull(item);
    lock (CommitGate.SyncRoot)
    {
      if (!item.Handle.IsValid || !playerId.IsValid || !epoch.IsValid
        || duration <= TimeSpan.Zero
        || duration.Ticks > DateTimeOffset.MaxValue.Ticks - now.Ticks
        || duration.Ticks > DateTimeOffset.MaxValue.UtcTicks - now.UtcTicks)
      {
        return ReservationResult.Rejected(
          ProtocolErrorCode.Forbidden, "The reservation identity or duration is invalid.");
      }

      if (_items is not null && (!_items.TryCapture(item.Handle.SlotId, out var currentItem)
        || currentItem is null || currentItem.Handle != item.Handle
        || currentItem.Revision != item.Revision))
      {
        return ReservationResult.Conflict("The captured world item is no longer current.");
      }

      _bySlot.TryGetValue(item.Handle.SlotId, out WorldItemReservation? current);
      if (current is not null && current.Handle == item.Handle
        && (current.FrameBased || current.ExpiresAt > now)
        && (current.PlayerId != playerId || current.Epoch != epoch))
      {
        return ReservationResult.Conflict("The world item is reserved by another session.");
      }

      if (!TryNextVersion(item.Handle, out CommitVersion revision))
      {
        return ReservationResult.Unavailable("The reservation revision space is exhausted.");
      }

      if (_items is not null && !_items.Projections.CanPublish([], [item.Handle], 0))
      {
        return ReservationResult.Unavailable("The projection sequence space is exhausted.");
      }

      if (owner is not null && (owner.Binding.PlayerHandle.WorldScope != item.Handle.WorldScope
        || owner.Validate().Status != OwnerCommitStatus.Applied))
      {
        return ReservationResult.Conflict("The captured authenticated owner binding changed.");
      }

      var reservation = new WorldItemReservation(
        item.Handle, playerId, epoch, now, now.Add(duration), revision,
        owner?.Binding.PlayerSlot);
      _bySlot[item.Handle.SlotId] = reservation;
      _versions[item.Handle] = revision;
      _items?.Projections.PublishOwnership(item.Handle, revision, reservation, item.Position);
      return ReservationResult.Applied(reservation);
    }
  }

  public bool TryGet(
    WorldItemHandle handle, DateTimeOffset now, out WorldItemReservation? reservation)
  {
    lock (CommitGate.SyncRoot)
    {
      reservation = _bySlot.GetValueOrDefault(handle.SlotId);
      if (reservation is not null && reservation.Handle == handle
        && (reservation.FrameBased || reservation.ExpiresAt > now))
      {
        return true;
      }

      reservation = null;
      return false;
    }
  }

  public bool IsReservedBy(
    WorldItemHandle handle, EntityUuid playerId, SessionEpoch epoch, DateTimeOffset now) =>
    TryGet(handle, now, out WorldItemReservation? reservation)
      && reservation is not null && reservation.PlayerId == playerId && reservation.Epoch == epoch;

  public bool Release(WorldItemHandle handle, EntityUuid playerId, SessionEpoch epoch)
  {
    lock (CommitGate.SyncRoot)
    {
      if (!_bySlot.TryGetValue(handle.SlotId, out WorldItemReservation? current)
        || current.Handle != handle || current.PlayerId != playerId || current.Epoch != epoch)
      {
        return false;
      }

      return Remove(current);
    }
  }

  public bool ReleaseAsServer(WorldItemHandle handle)
  {
    lock (CommitGate.SyncRoot)
    {
      return _bySlot.TryGetValue(handle.SlotId, out WorldItemReservation? current)
        && current.Handle == handle && Remove(current);
    }
  }

  public int ReleaseSession(SessionEpoch epoch)
  {
    lock (CommitGate.SyncRoot)
    {
      return RemoveWhere(value => value.Epoch == epoch);
    }
  }

  public int ReleaseWorldScope(WorldScopeId scope)
  {
    lock (CommitGate.SyncRoot)
    {
      return RemoveWhere(value => value.Handle.WorldScope == scope);
    }
  }

  public int Expire(DateTimeOffset now)
  {
    lock (CommitGate.SyncRoot)
    {
      return RemoveWhere(value => !value.FrameBased && value.ExpiresAt <= now);
    }
  }

  public IReadOnlyList<WorldItemReservation> CaptureWorld(WorldScopeId scope)
  {
    lock (CommitGate.SyncRoot)
    {
      return Array.AsReadOnly(_bySlot.Values.Where(value => value.Handle.WorldScope == scope)
        .OrderBy(value => value.Handle.SlotId).ToArray());
    }
  }

  public IReadOnlyList<WorldItemReservation> CaptureSession(SessionEpoch epoch)
  {
    lock (CommitGate.SyncRoot)
    {
      return Array.AsReadOnly(_bySlot.Values.Where(value => value.Epoch == epoch).ToArray());
    }
  }

  internal bool PrepareRelease(
    IReadOnlyList<WorldItemHandle> handles, out PreparedRelease prepared)
  {
    var entries = new List<ReleaseEntry>();
    foreach (WorldItemHandle handle in handles.Distinct())
    {
      WorldItemReservation? current = _bySlot.GetValueOrDefault(handle.SlotId);
      if (current is not null && current.Handle != handle)
      {
        prepared = new PreparedRelease(this, []);
        return false;
      }

      if (!TryNextVersion(handle, out CommitVersion revision))
      {
        prepared = new PreparedRelease(this, []);
        return false;
      }

      Vector2 position = CapturePosition(handle);
      entries.Add(new ReleaseEntry(handle, current, revision, position, null, true));
    }

    prepared = new PreparedRelease(this, entries.ToArray());
    return true;
  }

  internal bool PrepareChanges(
    IReadOnlyList<WorldItemHandle> releases, IReadOnlyList<WorldItemOwnershipChange> changes,
    out PreparedRelease prepared)
  {
    if (!PrepareRelease(releases, out prepared))
    {
      return false;
    }

    var entries = new List<ReleaseEntry>(prepared.Entries);
    var handles = releases.ToHashSet();
    foreach (WorldItemOwnershipChange change in changes)
    {
      WorldItemSnapshot item = change.Item;
      if (!handles.Add(item.Handle) || !item.Handle.IsValid
        || !ReferenceEquals(_bySlot.GetValueOrDefault(item.Handle.SlotId), change.Before)
        || change.Before is not null && change.Before.Handle != item.Handle
        || _items is null || !_items.TryCapture(item.Handle.SlotId, out var current)
        || current is null || current.Handle != item.Handle || current.Revision != item.Revision
        || !TryNextVersion(item.Handle, out CommitVersion revision))
      {
        return false;
      }

      WorldItemOwnerBindingCapture? owner = change.Owner;
      if (owner is not null && (owner.Binding.PlayerHandle.WorldScope != item.Handle.WorldScope
          || owner.Validate().Status != OwnerCommitStatus.Applied)
        || change.RetainOwner && (change.Before is null || change.PublishOwnership
          || change.RequestRelease || owner is not null)
        || owner is null && !change.RetainOwner
          && (change.RequestRelease || change.ReleaseCounter != 0)
        || change.RequestRelease && (change.ReleaseCounter != -1 || change.Before is null
          || owner!.Binding.Epoch != change.Before.Epoch
          || owner.Binding.PlayerHandle.Id != change.Before.PlayerId))
      {
        return false;
      }

      // Frame claims do not use wall-clock expiry; default timestamps carry no lease meaning.
      WorldItemReservation? after = change.RetainOwner
        ? change.Before! with { Revision = revision, ReleaseCounter = change.ReleaseCounter }
        : owner is null ? null : new WorldItemReservation(
          item.Handle, owner.Binding.PlayerHandle.Id, owner.Binding.Epoch, default, default,
          revision, owner.Binding.PlayerSlot, FrameBased: true,
          ReleaseCounter: change.ReleaseCounter);
      entries.Add(new ReleaseEntry(item.Handle, change.Before, revision, item.Position,
        after, change.PublishOwnership, change.RequestRelease, change.IgnoreEpoch));
    }

    prepared = new PreparedRelease(this, entries.ToArray());
    return true;
  }

  internal bool CanCommit(PreparedRelease prepared) =>
    ReferenceEquals(prepared.Owner, this) && !prepared.Committed
      && prepared.Entries.All(entry =>
        ReferenceEquals(_bySlot.GetValueOrDefault(entry.Handle.SlotId), entry.Before)
        && (!_versions.TryGetValue(entry.Handle, out CommitVersion version)
          ? entry.Revision == CommitVersion.Initial : version.Value + 1 == entry.Revision.Value));

  internal void CommitPrepared(PreparedRelease prepared, bool publish = true)
  {
    if (!CanCommit(prepared))
    {
      throw new InvalidOperationException("A reservation release was not validated at commit.");
    }

    foreach (ReleaseEntry entry in prepared.Entries)
    {
      if (entry.After is null)
      {
        _bySlot.Remove(entry.Handle.SlotId);
      }
      else
      {
        _bySlot[entry.Handle.SlotId] = entry.After;
      }
      _versions[entry.Handle] = entry.Revision;
      if (publish)
      {
        Publish(entry);
      }
    }

    prepared.Committed = true;
  }

  internal void Publish(PreparedRelease prepared)
  {
    foreach (ReleaseEntry entry in prepared.Entries)
    {
      Publish(entry);
    }
  }

  private void Publish(ReleaseEntry entry)
  {
    if (entry.PublishOwnership)
    {
      _items?.Projections.PublishOwnership(
        entry.Handle, entry.Revision, entry.After, entry.Position, entry.IgnoreEpoch);
    }
    if (entry.RequestRelease)
    {
      _items?.Projections.PublishOwnershipReleaseRequest(entry.After!);
    }
  }

  private bool TryNextVersion(WorldItemHandle handle, out CommitVersion revision)
  {
    if (!_versions.TryGetValue(handle, out CommitVersion current))
    {
      revision = CommitVersion.Initial;
      return true;
    }

    if (current.Value == long.MaxValue)
    {
      revision = current;
      return false;
    }

    revision = current.Next();
    return true;
  }

  private bool Remove(WorldItemReservation current)
  {
    if (!TryNextVersion(current.Handle, out CommitVersion revision)
      || _items is not null && !_items.Projections.CanPublish([], [current.Handle], 0))
    {
      return false;
    }

    Vector2 position = CapturePosition(current.Handle);
    _bySlot.Remove(current.Handle.SlotId);
    _versions[current.Handle] = revision;
    _items?.Projections.PublishOwnership(current.Handle, revision, null, position);
    return true;
  }

  private int RemoveWhere(Func<WorldItemReservation, bool> predicate)
  {
    WorldItemHandle[] handles = _bySlot.Values.Where(predicate)
      .Select(value => value.Handle).ToArray();
    if (handles.Length == 0)
    {
      return 0;
    }

    if (!PrepareRelease(handles, out PreparedRelease prepared)
      || _items is not null && !_items.Projections.CanPublish([], handles, 0))
    {
      return 0;
    }

    CommitPrepared(prepared);
    return handles.Length;
  }

  private Vector2 CapturePosition(WorldItemHandle handle)
  {
    return _items is not null && _items.TryCapture(handle.SlotId, out var item)
      && item is not null && item.Handle == handle ? item.Position : Vector2.Zero;
  }

  internal sealed record ReleaseEntry(
    WorldItemHandle Handle, WorldItemReservation? Before, CommitVersion Revision,
    Vector2 Position, WorldItemReservation? After, bool PublishOwnership,
    bool RequestRelease = false, SessionEpoch? IgnoreEpoch = null);

  internal sealed class PreparedRelease(
    WorldItemReservationStore owner, ReleaseEntry[] entries)
  {
    internal WorldItemReservationStore Owner { get; } = owner;
    internal IReadOnlyList<ReleaseEntry> Entries { get; } = Array.AsReadOnly(entries);
    internal bool Committed { get; set; }
  }
}
