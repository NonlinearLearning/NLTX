using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Rules;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Prepares the dedicated-server FindOwner pass and packet39 acknowledgement.</summary>
public sealed class WorldItemOwnershipCoordinator
{
  private readonly InventoryStore _inventories;
  private readonly WorldItemStore _items;
  private readonly WorldItemTransactionCoordinator _transactions;
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly WorldOwner _world;
  private readonly PlayerSpatialFactsService _spatialFacts;
  private readonly Func<SessionBinding, Player.Player, IPlayerPickupFactsCapture?> _captureSources;
  private readonly ItemPickupCatalog _catalog;
  private readonly WorldItemPickupPlanner _planner;

  internal WorldItemStore Items => _items;
  internal WorldItemReservationStore Reservations => _transactions.Reservations;

  public WorldItemOwnershipCoordinator(
    InventoryStore inventories, WorldItemStore items,
    WorldItemTransactionCoordinator transactions, PlayerOwner players,
    SessionAdmissionCoordinator sessions, WorldOwner world,
    PlayerSpatialFactsService spatialFacts,
    Func<SessionBinding, Player.Player, IPlayerPickupFactsCapture?> captureSources,
    ItemPickupCatalog catalog, WorldItemPickupPlanner planner)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(transactions);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(spatialFacts);
    ArgumentNullException.ThrowIfNull(captureSources);
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(planner);
    if (!ReferenceEquals(items, transactions.Items)
      || !ReferenceEquals(inventories, transactions.Inventories)
      || !ReferenceEquals(inventories.CommitGate, items.CommitGate))
    {
      throw new ArgumentException("Ownership must use the transaction's authoritative stores.");
    }
    _inventories = inventories;
    _items = items;
    _transactions = transactions;
    _players = players;
    _sessions = sessions;
    _world = world;
    _spatialFacts = spatialFacts;
    _captureSources = captureSources;
    _catalog = catalog;
    _planner = planner;
  }

  /// <summary>
  /// Called once after motion, in the same exclusive ServerLoop window. frameCounter is the
  /// real server netPlayCounter; repeated calls are not deduplicated by a local clock.
  /// </summary>
  public OwnerCommitResult PrepareFrame(
    WorldScopeId scope, long frameCounter, WorldSpatialSnapshot tiles,
    IWorldItemEmergencyStackingCapture? emergency,
    out WorldItemTransactionCoordinator.PreparedTransaction? transaction)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    transaction = null;
    if (!scope.IsValid || frameCounter < 0)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden,
        "Ownership requires a valid world scope and server frame counter.");
    }
    lock (_items.CommitGate.SyncRoot)
    {
      if (emergency is not null)
      {
        OwnerCommitResult validation = ValidateEmergency(scope, emergency);
        if (validation.Status != OwnerCommitStatus.Applied)
        {
          return validation;
        }
      }
      OwnerCommitResult result = CaptureRoster(tiles, out var roster);
      if (roster is null)
      {
        return result;
      }
      var changes = new List<WorldItemOwnershipChange>();
      var claims = _transactions.Reservations.CaptureWorld(scope)
        .ToDictionary(claim => claim.Handle);
      foreach (WorldItemSnapshot item in _items.CaptureWorld(scope))
      {
        WorldItemReservation? before = claims.GetValueOrDefault(item.Handle);
        if (before is not null && !before.FrameBased)
        {
          return Unavailable("A duration lease cannot be advanced as a frame ownership claim.");
        }
        int counter = before is { ReleaseCounter: >= 0 }
          ? unchecked(before.ReleaseCounter + 1) : before?.ReleaseCounter ?? 0;
        Candidate? old = roster.Find(before);
        bool scan = before is null ? frameCounter % 5 == 0
          : old is null || counter % 300 == 0;
        if (scan)
        {
          result = PrepareSelection(item, before, counter, roster, tiles, emergency,
            acknowledgement: false, out var change);
          if (result.Status != OwnerCommitStatus.Applied)
          {
            return result;
          }
          if (change is not null)
          {
            changes.Add(change);
          }
        }
        else if (before is not null && before.ReleaseCounter != counter)
        {
          changes.Add(new WorldItemOwnershipChange(item, before, old!.Binding, counter, false));
        }
      }
      return PrepareTransaction(changes, roster, emergency, null, out transaction);
    }
  }

  /// <summary>The authenticated current owner may acknowledge without a pending39 flag.</summary>
  public OwnerCommitResult PrepareAcknowledgement(
    WorldItemHandle handle, WorldItemOwnerBindingCapture sender, WorldSpatialSnapshot tiles,
    IWorldItemEmergencyStackingCapture? emergency,
    out WorldItemTransactionCoordinator.PreparedTransaction? transaction)
  {
    ArgumentNullException.ThrowIfNull(sender);
    ArgumentNullException.ThrowIfNull(tiles);
    transaction = null;
    lock (_items.CommitGate.SyncRoot)
    {
      if (!_items.TryCapture(handle.SlotId, out var item) || item is null || item.Handle != handle
        || sender.Binding.PlayerHandle.WorldScope != handle.WorldScope)
      {
        return Conflict("The acknowledged world item changed.");
      }
      WorldItemReservation? before = _transactions.Reservations.CaptureWorld(handle.WorldScope)
        .FirstOrDefault(claim => claim.Handle == handle);
      if (before is null || before.PlayerId != sender.Binding.PlayerHandle.Id
        || before.Epoch != sender.Binding.Epoch || before.WirePlayerSlot != sender.Binding.PlayerSlot)
      {
        return new OwnerCommitResult(OwnerCommitStatus.Stale, null,
          ProtocolErrorCode.StaleEntity, "The sender no longer owns the world item.");
      }
      if (!before.FrameBased)
      {
        return Unavailable("A duration lease cannot be acknowledged as frame ownership.");
      }
      OwnerCommitResult result = sender.Validate();
      if (result.Status != OwnerCommitStatus.Applied)
      {
        return result;
      }
      if (emergency is not null)
      {
        result = ValidateEmergency(handle.WorldScope, emergency);
        if (result.Status != OwnerCommitStatus.Applied)
        {
          return result;
        }
      }
      result = CaptureRoster(tiles, out var roster);
      if (roster is null)
      {
        return result;
      }
      result = PrepareSelection(item, before, 0, roster, tiles, emergency,
        acknowledgement: true, out var change);
      if (result.Status != OwnerCommitStatus.Applied || change is null)
      {
        return result;
      }
      return PrepareTransaction([change], roster, emergency, sender, out transaction);
    }
  }

  private OwnerCommitResult PrepareTransaction(
    IReadOnlyList<WorldItemOwnershipChange> changes, RosterGuard roster,
    IWorldItemEmergencyStackingCapture? emergency, WorldItemOwnerBindingCapture? sender,
    out WorldItemTransactionCoordinator.PreparedTransaction? transaction)
  {
    var participants = new List<IWorldItemTransactionParticipant> { roster };
    participants.AddRange(changes.Where(change => change.Owner is not null)
      .Select(change => change.Owner!).Distinct());
    if (sender is not null && !participants.Contains(sender))
    {
      participants.Add(sender);
    }
    if (emergency is not null)
    {
      participants.Add(emergency);
    }
    return _transactions.Prepare([], [], [], changes, null, participants, out transaction);
  }

  private OwnerCommitResult PrepareSelection(
    WorldItemSnapshot item, WorldItemReservation? before, int counter, RosterGuard roster,
    WorldSpatialSnapshot tiles, IWorldItemEmergencyStackingCapture? emergency,
    bool acknowledgement, out WorldItemOwnershipChange? change)
  {
    change = null;
    Candidate? old = acknowledgement ? null : roster.Find(before);
    WorldItemRuntime runtime = item.Runtime ?? new WorldItemRuntime();
    if (runtime.KeepTime > 0)
    {
      if (acknowledgement || before is not null && counter != before.ReleaseCounter)
      {
        change = new WorldItemOwnershipChange(item, before, old?.Binding,
          acknowledgement ? 0 : counter, acknowledgement,
          IgnoreEpoch: acknowledgement ? before!.Epoch : null,
          RetainOwner: !acknowledgement && old is null);
      }
      return OwnerCommitResult.Applied(item.Revision);
    }
    if (emergency is null || emergency.WorldScope != item.Handle.WorldScope)
    {
      return Unavailable("FindOwner requires the captured authoritative emergency-transfer state.");
    }
    Candidate? selected = null;
    if (!emergency.HasPendingTransfer(item.Handle) && item.ShimmerTime <= 0
      && (item.Item.Type != 267 || runtime.OwnIgnore == -1))
    {
      float bestDistance = 1920f;
      foreach (Candidate candidate in roster.Candidates)
      {
        if (candidate.Dead || candidate.Binding.Binding.PlayerHandle.WorldScope != item.Handle.WorldScope
          || candidate.Binding.Binding.PlayerSlot == runtime.OwnIgnore)
        {
          continue;
        }
        OwnerCommitResult result = CaptureCandidate(candidate, item, out bool canTake);
        if (result.Status != OwnerCommitStatus.Applied)
        {
          return result;
        }
        if (!canTake)
        {
          continue;
        }
        PlayerSpatialSnapshot geometry = candidate.Geometry!;
        PlayerPickupSourceFacts facts = candidate.Sources!.Facts;
        // Preserve the source's integer halves and left-associative Single arithmetic.
        float distance = MathF.Abs(geometry.Position.X + (float)(geometry.Width / 2)
          - item.Position.X - 8f) + MathF.Abs(geometry.Position.Y
          + (float)(geometry.Height / 2) - item.Position.Y - 16f);
        if (facts.ManaMagnet && item.Item.Type is 184 or 1735 or 1868)
        {
          distance -= 300f;
        }
        if (facts.LifeMagnet && item.Item.Type is 58 or 1734 or 1867)
        {
          distance -= 250f;
        }
        if (item.Item.Type == 4143)
        {
          distance -= 300f;
        }
        if (distance < bestDistance)
        {
          bestDistance = distance;
          selected = candidate;
        }
      }
      if (selected is not null && !IntersectsGrabRange(selected, item))
      {
        if (!TryHasHopper(tiles, item, out bool hopper))
        {
          return Unavailable("The Hopper range is outside the captured world tiles.");
        }
        if (hopper)
        {
          selected = null;
        }
      }
    }
    bool sameOwner = !acknowledgement && before is not null && selected is not null
      && selected.Binding.Binding.Epoch == before.Epoch
      && selected.Binding.Binding.PlayerHandle.Id == before.PlayerId;
    if (sameOwner || before is null && selected is null && !acknowledgement)
    {
      if (before is not null && before.ReleaseCounter != counter)
      {
        change = new WorldItemOwnershipChange(item, before, selected!.Binding, counter, false);
      }
    }
    else if (old is not null)
    {
      if (counter >= 0)
      {
        change = new WorldItemOwnershipChange(item, before, old.Binding, -1, false, true);
      }
      else if (before!.ReleaseCounter != counter)
      {
        change = new WorldItemOwnershipChange(item, before, old.Binding, counter, false);
      }
    }
    else
    {
      change = new WorldItemOwnershipChange(item, before, selected?.Binding, 0, true,
        IgnoreEpoch: acknowledgement && selected is null ? before!.Epoch : null);
    }
    return OwnerCommitResult.Applied(item.Revision);
  }

  private OwnerCommitResult CaptureCandidate(
    Candidate candidate, WorldItemSnapshot item, out bool canTake)
  {
    canTake = false;
    SessionBinding binding = candidate.Binding.Binding;
    if (candidate.Sources is null)
    {
      candidate.Sources = _captureSources.Invoke(binding, candidate.Player);
      candidate.CapturedFacts = candidate.Sources?.Facts;
      candidate.SourceRevision = candidate.Sources?.SourceRevision;
    }
    IPlayerPickupFactsCapture? sources = candidate.Sources;
    if (sources is null || sources.Epoch != binding.Epoch
      || sources.PlayerHandle != binding.PlayerHandle || sources.PlayerVersion != candidate.Version
      || sources.SourceRevision is not { IsValid: true } || sources.Facts is null)
    {
      return Unavailable("Verified FindOwner acceptance and magnet sources are unavailable.");
    }
    OwnerCommitResult sourceValidation = sources.Validate();
    if (sourceValidation.Status != OwnerCommitStatus.Applied)
    {
      return sourceValidation;
    }
    bool pickup = ItemContentFacts.IsPickup(item.Item.Type);
    canTake = pickup;
    if (!pickup)
    {
      if (candidate.Inventory is null)
      {
        _inventories.TryCapture(binding.PlayerHandle.Id, out var inventory);
        candidate.Inventory = inventory;
      }
      if (candidate.Inventory is null
        || !_catalog.TryGet(item.Item.Type, out var itemFacts) || itemFacts is null
        || !_planner.TryGetAttractionSpace(candidate.Inventory, item.Item, itemFacts,
          sources.Facts.VoidVaultEnabled, out canTake, out _))
      {
        return Unavailable("Verified FindOwner ItemSpace content or inventory is unavailable.");
      }
    }
    canTake &= !sources.Facts.PreventAllItemPickups || pickup;
    if (canTake && candidate.Geometry is null)
    {
      OwnerCommitResult result = _spatialFacts.Capture(binding.Epoch, out var geometry);
      if (result.Status != OwnerCommitStatus.Applied || geometry is null)
      {
        return result;
      }
      if (geometry.Version != candidate.Version || geometry.PlayerHandle != binding.PlayerHandle
        || !ReferenceEquals(geometry.Binding, binding))
      {
        return Conflict("FindOwner geometry and source captures disagree.");
      }
      candidate.Geometry = geometry;
    }
    return OwnerCommitResult.Applied(candidate.Version);
  }

  private OwnerCommitResult CaptureRoster(
    WorldSpatialSnapshot tiles, out RosterGuard? roster)
  {
    roster = null;
    if (!_world.IsReady || !_world.IsCurrentWorldRevision(tiles.WorldEpoch, tiles.Version))
    {
      return Conflict("The ownership world capture is no longer current.");
    }
    SessionEpoch[] epochs = _sessions.CaptureActiveSessionEpochs().ToArray();
    var candidates = new List<Candidate>();
    foreach (SessionEpoch epoch in epochs)
    {
      if (!_sessions.TryGetBinding(epoch, out var binding) || binding is null
        || !_players.TryGetBoundPlayer(epoch, _sessions, out var player) || player is null
        || !WorldItemOwnerBindingCapture.TryCapture(_sessions, binding.PlayerHandle.Id,
          epoch, binding.PlayerHandle.WorldScope, out var owner) || owner is null)
      {
        return Conflict("An Active roster entry has no exact authenticated player binding.");
      }
      candidates.Add(new Candidate(owner, player));
    }
    var captured = new RosterGuard(this, epochs, candidates.ToArray(), tiles);
    OwnerCommitResult result = captured.Validate();
    if (result.Status == OwnerCommitStatus.Applied)
    {
      roster = captured;
    }
    return result;
  }

  private static bool IntersectsGrabRange(Candidate candidate, WorldItemSnapshot item)
  {
    PlayerSpatialSnapshot player = candidate.Geometry!;
    int range = candidate.Sources!.Facts.GetItemGrabRange(item.Item.Type);
    int left = (int)player.Position.X - range;
    int top = (int)player.Position.Y - range;
    int itemX = (int)item.Position.X;
    int itemY = (int)item.Position.Y;
    return itemX < left + player.Width + range * 2 && itemX + 16 > left
      && itemY < top + player.Height + range * 2 && itemY + 16 > top;
  }

  private static bool TryHasHopper(
    WorldSpatialSnapshot tiles, WorldItemSnapshot item, out bool hopper)
  {
    hopper = false;
    if (ItemContentFacts.IsPickup(item.Item.Type)
      || item.Item.Type is 5013 or 6135 or 6136 or 6143)
    {
      return true;
    }
    WorldGeometry geometry = tiles.Header.Geometry;
    if (geometry.Width <= 0 || geometry.Height <= 0)
    {
      return false;
    }
    int x = (int)item.Position.X;
    int y = (int)item.Position.Y;
    // Utils.ClampedInWorld uses maxTilesX for both axes in this exact source version.
    int left = Math.Clamp((x - 96) >> 4, 0, geometry.Width - 1);
    int right = Math.Clamp((x + 16 + 96) >> 4, 0, geometry.Width - 1);
    int top = Math.Clamp((y - 96) >> 4, 0, geometry.Width - 1);
    int bottom = Math.Clamp((y + 16 + 96) >> 4, 0, geometry.Width - 1);
    if (bottom >= geometry.Height || left < tiles.Bounds.OriginX || top < tiles.Bounds.OriginY
      || right >= tiles.Bounds.OriginX + tiles.Bounds.Width
      || bottom >= tiles.Bounds.OriginY + tiles.Bounds.Height)
    {
      return false;
    }
    for (int tileX = left; tileX <= right; tileX++)
    {
      for (int tileY = top; tileY <= bottom; tileY++)
      {
        WorldTile tile = tiles.GetTile(new TileCoordinate(tileX, tileY));
        if (tile.Active && tile.Type is 21 or 467 && tile.Wires != TileWireMask.None)
        {
          hopper = true;
          return true;
        }
      }
    }
    return true;
  }

  private static OwnerCommitResult Conflict(string detail) => new(
    OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict, detail);

  private static OwnerCommitResult Unavailable(string detail) => new(
    OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);

  private static OwnerCommitResult ValidateEmergency(
    WorldScopeId scope, IWorldItemEmergencyStackingCapture emergency)
  {
    return emergency.WorldScope == scope ? emergency.Validate()
      : Conflict("The emergency-transfer capture belongs to another world scope.");
  }

  private sealed class Candidate(WorldItemOwnerBindingCapture binding, Player.Player player)
  {
    internal WorldItemOwnerBindingCapture Binding { get; } = binding;
    internal Player.Player Player { get; } = player;
    internal CommitVersion Version { get; } = player.Version;
    internal bool Dead { get; } = player.IsDead;
    internal IPlayerPickupFactsCapture? Sources { get; set; }
    internal PlayerSpatialSnapshot? Geometry { get; set; }
    internal InventorySnapshot? Inventory { get; set; }
    internal PlayerPickupSourceFacts? CapturedFacts { get; set; }
    internal PlayerSourceRevision? SourceRevision { get; set; }
  }

  private sealed class RosterGuard(
    WorldItemOwnershipCoordinator owner, SessionEpoch[] epochs, Candidate[] candidates,
    WorldSpatialSnapshot tiles) : IWorldItemTransactionParticipant
  {
    internal IReadOnlyList<Candidate> Candidates { get; } = Array.AsReadOnly(candidates);

    internal Candidate? Find(WorldItemReservation? claim)
    {
      return claim is null ? null : Candidates.FirstOrDefault(candidate =>
        candidate.Binding.Binding.Epoch == claim.Epoch
        && candidate.Binding.Binding.PlayerHandle.Id == claim.PlayerId
        && candidate.Binding.Binding.PlayerSlot == claim.WirePlayerSlot);
    }

    public OwnerCommitResult Validate()
    {
      if (!owner._world.IsReady
        || !owner._world.IsCurrentWorldRevision(tiles.WorldEpoch, tiles.Version)
        || !owner._sessions.CaptureActiveSessionEpochs().SequenceEqual(epochs))
      {
        return Conflict("The ownership world or full Active roster changed.");
      }
      foreach (Candidate candidate in Candidates)
      {
        SessionBinding binding = candidate.Binding.Binding;
        OwnerCommitResult result = candidate.Binding.Validate();
        if (result.Status != OwnerCommitStatus.Applied)
        {
          return result;
        }
        if (!owner._players.TryGet(binding.PlayerHandle, out var current)
          || !ReferenceEquals(current, candidate.Player) || current!.Epoch != binding.Epoch
          || current.Version != candidate.Version || current.IsDead != candidate.Dead)
        {
          return Conflict("A scanned or skipped ownership player changed.");
        }
        if (candidate.Sources is { } sources)
        {
          if (sources.Epoch != binding.Epoch || sources.PlayerHandle != binding.PlayerHandle
            || sources.PlayerVersion != candidate.Version
            || sources.SourceRevision != candidate.SourceRevision
            || sources.Facts != candidate.CapturedFacts)
          {
            return Conflict("Captured ownership acceptance or magnet sources changed.");
          }
          result = sources.Validate();
          if (result.Status != OwnerCommitStatus.Applied)
          {
            return result;
          }
        }
        if (candidate.Geometry is { } geometry)
        {
          result = geometry.Validate();
          if (result.Status != OwnerCommitStatus.Applied)
          {
            return result;
          }
        }
        if (candidate.Inventory is { } inventory
          && (!owner._inventories.TryCapture(inventory.PlayerId, out var slots) || slots is null
            || slots.Revision != inventory.Revision || !slots.Slots.SequenceEqual(inventory.Slots)))
        {
          return Conflict("An ownership candidate inventory or its slot generation changed.");
        }
      }
      return OwnerCommitResult.Applied(tiles.Version);
    }

    public void Commit() { }
  }
}
