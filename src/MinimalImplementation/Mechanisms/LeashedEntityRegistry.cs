using System.Numerics;
using Terraria.WorldStorage;
using LeashedEntityMessage = MinimalImplementation.Server.Protocol.Version4ModulePayloads.LeashedEntityMessage;
using LeashedEntityState = MinimalImplementation.Server.Protocol.Version4ModulePayloads.LeashedEntityState;
using KiteLeashedEntityState = MinimalImplementation.Server.Protocol.Version4ModulePayloads.KiteLeashedEntityState;
using CritterLeashedEntityState = MinimalImplementation.Server.Protocol.Version4ModulePayloads.CritterLeashedEntityState;

namespace MinimalImplementation.Server.Mechanisms;

public enum LeashedEntityChangeKind
{
  Created,
  Updated,
  Removed,
  NoOp
}

public sealed record LeashedEntityRuntimeSnapshot(
  MechanismEntityHandle Handle,
  int Slot,
  byte Type,
  TileCoordinate Anchor,
  MechanismSectionCoordinate Section,
  LeashedEntityState State,
  long Revision,
  LeashedEntityMotionState? Motion = null,
  bool Active = true);

public sealed record LeashedEntityChangeResult(
  LeashedEntityChangeKind Kind,
  LeashedEntityRuntimeSnapshot? Current,
  LeashedEntityRuntimeSnapshot? Previous);

/// <summary>Owns bounded Version4 leashed-entity slots and their section index.</summary>
public sealed class LeashedEntityRegistry
{
  /// <summary>
  /// Prepares TE OnRemoved: detach the anchor and mark inactive without removing its slot.
  /// The next active-section tick owns identity retirement and the Remove projection.
  /// </summary>
  public sealed class DeactivationPreparation
  {
    private readonly LeashedEntityRegistry _owner;
    private readonly long _baseRevision;
    private readonly long _nextRevision;
    private readonly Dictionary<int, LeashedEntityRuntimeSnapshot> _bySlot;
    private readonly Dictionary<TileCoordinate, int> _slotByAnchor;
    private bool _committed;

    public LeashedEntityRuntimeSnapshot? Previous { get; }

    public LeashedEntityRuntimeSnapshot? Current { get; }

    internal DeactivationPreparation(
      LeashedEntityRegistry owner,
      LeashedEntityRuntimeSnapshot? previous)
    {
      _owner = owner;
      _baseRevision = owner.Revision;
      _nextRevision = previous is null ? owner.Revision : checked(owner.Revision + 1);
      _bySlot = new(owner._bySlot);
      _slotByAnchor = new(owner._slotByAnchor);
      Previous = previous;
      if (previous is not null)
      {
        Current = previous with { Active = false, Revision = checked(previous.Revision + 1) };
        _bySlot[previous.Slot] = Current;
        _slotByAnchor.Remove(previous.Anchor);
      }
    }

    public bool Validate()
    {
      return !_committed && _owner.Revision == _baseRevision &&
        (Previous is null || _owner._identityAuthority.IsActive(Previous.Handle));
    }

    /// <summary>Requires all participants validated in one exclusive owner window.</summary>
    public void Commit()
    {
      if (_committed)
      {
        throw new InvalidOperationException("The leashed deactivation was already committed.");
      }
      _owner._bySlot = _bySlot;
      _owner._slotByAnchor = _slotByAnchor;
      _owner.Revision = _nextRevision;
      _committed = true;
    }
  }

  /// <summary>Prepared Spawn(false) states for one section, without identity changes.</summary>
  public sealed class SectionRespawnPreparation
  {
    private readonly LeashedEntityRegistry _owner;
    private readonly long _baseRevision;
    private readonly long _nextRevision;
    private readonly Dictionary<int, LeashedEntityRuntimeSnapshot> _bySlot;
    private bool _committed;

    public IReadOnlyList<LeashedEntityRuntimeSnapshot> Entities { get; }

    internal SectionRespawnPreparation(
      LeashedEntityRegistry owner,
      Dictionary<int, LeashedEntityRuntimeSnapshot> bySlot,
      IReadOnlyList<LeashedEntityRuntimeSnapshot> entities)
    {
      _owner = owner;
      _baseRevision = owner.Revision;
      _nextRevision = entities.Count == 0 ? owner.Revision : checked(owner.Revision + 1);
      _bySlot = bySlot;
      Entities = entities;
    }

    public bool Validate()
    {
      return !_committed && _owner.Revision == _baseRevision &&
        Entities.All(entity => _owner._identityAuthority.IsActive(entity.Handle));
    }

    /// <summary>Requires validation in the same exclusive owner window.</summary>
    public void Commit()
    {
      if (_committed)
      {
        throw new InvalidOperationException("The leashed section respawn was already committed.");
      }
      _owner._bySlot = _bySlot;
      _owner.Revision = _nextRevision;
      _committed = true;
    }
  }

  /// <summary>Prepares every active-section motion and removal for one atomic game tick.</summary>
  public sealed class SectionTickPreparation
  {
    private readonly LeashedEntityRegistry _owner;
    private readonly long _baseRevision;
    private readonly long _nextRevision;
    private readonly IReadOnlyList<LeashedEntityRuntimeSnapshot> _expectedEntities;
    private readonly IReadOnlyList<LeashedEntityChangeResult> _changes;
    private readonly MechanismEntityHandle[] _retiredIdentities;
    private readonly Dictionary<MechanismSectionCoordinate, List<int>?> _sectionSlotsAfterRemoval;
    private bool _committed;

    public IReadOnlyList<LeashedEntityChangeResult> Changes => _changes;

    internal SectionTickPreparation(
      LeashedEntityRegistry owner,
      IReadOnlyList<LeashedEntityRuntimeSnapshot> expectedEntities,
      IReadOnlyList<LeashedEntityChangeResult> changes,
      MechanismEntityHandle[] retiredIdentities,
      Dictionary<MechanismSectionCoordinate, List<int>?> sectionSlotsAfterRemoval)
    {
      _owner = owner;
      _baseRevision = owner.Revision;
      _nextRevision = checked(owner.Revision + changes.Count);
      _expectedEntities = expectedEntities;
      _changes = changes;
      _retiredIdentities = retiredIdentities;
      _sectionSlotsAfterRemoval = sectionSlotsAfterRemoval;
    }

    public bool Validate()
    {
      if (_committed || _owner.Revision != _baseRevision)
      {
        return false;
      }

      foreach (LeashedEntityRuntimeSnapshot expected in _expectedEntities)
      {
        if (!_owner._bySlot.TryGetValue(expected.Slot, out LeashedEntityRuntimeSnapshot? current) ||
            !Equals(current, expected) || !_owner._identityAuthority.IsActive(expected.Handle))
        {
          return false;
        }
      }

      HashSet<int> changedSlots = [];
      foreach (LeashedEntityChangeResult change in _changes)
      {
        if (change.Kind == LeashedEntityChangeKind.Updated)
        {
          if (change.Current is null || change.Previous is null ||
              !change.Current.Active || change.Current.Handle != change.Previous.Handle ||
              !changedSlots.Add(change.Current.Slot) ||
              !_owner._identityAuthority.IsActive(change.Current.Handle))
          {
            return false;
          }
          continue;
        }

        if (change.Kind != LeashedEntityChangeKind.Removed || change.Current is not null ||
            change.Previous is not LeashedEntityRuntimeSnapshot previous || previous.Active ||
            !changedSlots.Add(previous.Slot) ||
            (_owner._slotByAnchor.TryGetValue(previous.Anchor, out int anchorSlot) &&
              anchorSlot == previous.Slot) ||
            !_owner._slotsBySection.TryGetValue(previous.Section, out List<int>? sectionSlots) ||
            sectionSlots.Count(slot => slot == previous.Slot) != 1 ||
            !_owner._identityAuthority.IsActive(previous.Handle))
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>Retires inactive identities, then applies the already-prepared tick changes.</summary>
    public IReadOnlyList<LeashedEntityChangeResult> Commit()
    {
      if (!Validate())
      {
        throw new InvalidOperationException("The leashed section tick became stale.");
      }
      if (_retiredIdentities.Length > 0 &&
          !_owner._identityAuthority.TryRetireMany(_retiredIdentities))
      {
        throw new InvalidOperationException(
          "Inactive leashed identities could not be retired for the section tick.");
      }

      foreach (LeashedEntityChangeResult change in _changes)
      {
        if (change.Kind == LeashedEntityChangeKind.Updated)
        {
          LeashedEntityRuntimeSnapshot current = change.Current!;
          _owner._bySlot[current.Slot] = current;
        }
        else
        {
          _owner._bySlot.Remove(change.Previous!.Slot);
        }
      }
      foreach (KeyValuePair<MechanismSectionCoordinate, List<int>?> update in
          _sectionSlotsAfterRemoval)
      {
        if (update.Value is null)
        {
          _owner._slotsBySection.Remove(update.Key);
        }
        else
        {
          _owner._slotsBySection[update.Key] = update.Value;
        }
      }
      _owner.Revision = _nextRevision;
      _committed = true;
      return _changes;
    }
  }

  public const int DefaultMaximumSlots = 65_536;

  private readonly MechanismFactsCatalog _facts;
  private readonly IMechanismIdentityAuthority _identityAuthority;
  private readonly Func<MechanismEntityHandle> _identityFactory;
  private readonly long _worldScope;
  private readonly int _maximumSlots;
  private Dictionary<int, LeashedEntityRuntimeSnapshot> _bySlot = [];
  private Dictionary<TileCoordinate, int> _slotByAnchor = [];
  private Dictionary<MechanismSectionCoordinate, List<int>> _slotsBySection = [];

  public LeashedEntityRegistry(
    MechanismFactsCatalog facts,
    IMechanismIdentityAuthority identityAuthority,
    Func<MechanismEntityHandle> identityFactory,
    long worldScope,
    int maximumSlots = DefaultMaximumSlots)
  {
    _facts = facts ?? throw new ArgumentNullException(nameof(facts));
    _identityAuthority = identityAuthority
      ?? throw new ArgumentNullException(nameof(identityAuthority));
    _identityFactory = identityFactory ?? throw new ArgumentNullException(nameof(identityFactory));
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    if (maximumSlots is < 1 or > DefaultMaximumSlots)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumSlots));
    }
    _worldScope = worldScope;
    _maximumSlots = maximumSlots;
  }

  public long WorldScope => _worldScope;

  public int MaximumSlots => _maximumSlots;

  public int Count => _bySlot.Count;

  public long Revision { get; private set; } = 1;

  public IReadOnlyList<LeashedEntityRuntimeSnapshot> Capture()
  {
    LeashedEntityRuntimeSnapshot[] snapshots = _bySlot.Values
      .OrderBy(entity => entity.Slot)
      .ToArray();
    foreach (LeashedEntityRuntimeSnapshot snapshot in snapshots)
    {
      EnsureIdentityActive(snapshot.Handle);
    }
    return Array.AsReadOnly(snapshots);
  }

  public IReadOnlyList<LeashedEntityRuntimeSnapshot> CaptureSection(
    MechanismSectionCoordinate section)
  {
    if (!_slotsBySection.TryGetValue(section, out List<int>? slots))
    {
      return Array.Empty<LeashedEntityRuntimeSnapshot>();
    }
    LeashedEntityRuntimeSnapshot[] snapshots = slots
      .Select(slot => _bySlot[slot])
      .ToArray();
    foreach (LeashedEntityRuntimeSnapshot snapshot in snapshots)
    {
      EnsureIdentityActive(snapshot.Handle);
    }
    return Array.AsReadOnly(snapshots);
  }

  public bool TryGet(int slot, out LeashedEntityRuntimeSnapshot? entity)
  {
    return _bySlot.TryGetValue(slot, out entity);
  }

  public bool TryGetAt(TileCoordinate anchor, out LeashedEntityRuntimeSnapshot? entity)
  {
    entity = null;
    if (!_slotByAnchor.TryGetValue(anchor, out int slot))
    {
      return false;
    }
    if (!_bySlot.TryGetValue(slot, out entity) || entity is null ||
        !entity.Active || entity.Anchor != anchor ||
        !_slotsBySection.TryGetValue(entity.Section, out List<int>? sectionSlots) ||
        sectionSlots is null || sectionSlots.Count(sectionSlot => sectionSlot == slot) != 1)
    {
      entity = null;
      throw new InvalidOperationException("The leashed registry indexes are inconsistent.");
    }
    EnsureIdentityActive(entity.Handle);
    return true;
  }

  /// <summary>Creates a local entity in the first free slot, retaining its real spawn motion.</summary>
  public LeashedEntityChangeResult Spawn(
    byte type,
    TileCoordinate anchor,
    MechanismSectionCoordinate section,
    LeashedEntitySpawnState spawn)
  {
    ArgumentNullException.ThrowIfNull(spawn);
    ValidateMotion(spawn.Motion);
    int slot = 0;
    while (slot < _maximumSlots && _bySlot.ContainsKey(slot))
    {
      slot++;
    }
    if (slot == _maximumSlots)
    {
      throw new InvalidOperationException("The leashed entity slots are exhausted.");
    }
    var message = new LeashedEntityMessage(1, slot, type,
      checked((short)anchor.X), checked((short)anchor.Y), spawn.FullState);
    return ApplyFull(message, section, spawn.Motion);
  }

  public LeashedEntityChangeResult RemoveAt(TileCoordinate anchor)
  {
    return _slotByAnchor.TryGetValue(anchor, out int slot)
      ? Remove(slot)
      : new LeashedEntityChangeResult(LeashedEntityChangeKind.NoOp, null, null);
  }

  public DeactivationPreparation PrepareDeactivateAt(TileCoordinate anchor)
  {
    TryGetAt(anchor, out LeashedEntityRuntimeSnapshot? previous);
    if (previous is not null && !previous.Active)
    {
      throw new InvalidOperationException("The leashed anchor index contains an inactive entity.");
    }
    return new DeactivationPreparation(this, previous);
  }

  public bool DeactivateAt(TileCoordinate anchor)
  {
    DeactivationPreparation preparation = PrepareDeactivateAt(anchor);
    if (!preparation.Validate())
    {
      throw new InvalidOperationException("The leashed deactivation became stale.");
    }
    preparation.Commit();
    return preparation.Previous is not null;
  }

  public SectionRespawnPreparation PrepareSectionRespawn(
    MechanismSectionCoordinate section,
    long expectedRevision,
    IReadOnlyDictionary<int, LeashedEntitySpawnState> spawns)
  {
    ValidateSection(section);
    ArgumentNullException.ThrowIfNull(spawns);
    if (Revision != expectedRevision)
    {
      throw new InvalidOperationException("The leashed section respawn snapshot is stale.");
    }
    IReadOnlyList<LeashedEntityRuntimeSnapshot> previous = CaptureSection(section);
    int activeCount = previous.Count(entity => entity.Active);
    if (spawns.Count != activeCount)
    {
      throw new ArgumentException("Every active section entity requires a spawn state.",
        nameof(spawns));
    }
    var candidates = new Dictionary<int, LeashedEntityRuntimeSnapshot>(_bySlot);
    var entities = new LeashedEntityRuntimeSnapshot[previous.Count];
    for (int index = 0; index < previous.Count; index++)
    {
      LeashedEntityRuntimeSnapshot before = previous[index];
      if (!before.Active)
      {
        entities[index] = before;
        continue;
      }
      if (!spawns.TryGetValue(before.Slot, out LeashedEntitySpawnState? spawn) || spawn is null)
      {
        throw new ArgumentException("A retained leashed slot has no spawn state.", nameof(spawns));
      }
      ValidateState(before.Type, spawn.FullState, full: true);
      if (!Equals(Merge(before.State, spawn.FullState), spawn.FullState))
      {
        throw new ArgumentException("Section respawn cannot change the entity defaults.",
          nameof(spawns));
      }
      ValidateMotionProjection(before.Anchor, spawn.FullState, spawn.Motion);
      if (before.Motion is not null && spawn.Motion.LastTick < before.Motion.LastTick)
      {
        throw new ArgumentException("A section respawn cannot move its tick backwards.",
          nameof(spawns));
      }
      LeashedEntityRuntimeSnapshot current = before with
      {
        State = spawn.FullState,
        Motion = spawn.Motion,
        Revision = checked(before.Revision + 1)
      };
      entities[index] = current;
      candidates[before.Slot] = current;
    }
    return new SectionRespawnPreparation(this, candidates, Array.AsReadOnly(entities));
  }

  /// <summary>Prepares motion and delayed removal for all captured active-section slots.</summary>
  public SectionTickPreparation PrepareSectionTick(
    long expectedRevision,
    IReadOnlyList<LeashedEntityRuntimeSnapshot> expectedEntities,
    IReadOnlyDictionary<int, LeashedEntityMotionStep> motionSteps,
    IReadOnlyCollection<int> removalSlots)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedRevision);
    ArgumentNullException.ThrowIfNull(expectedEntities);
    ArgumentNullException.ThrowIfNull(motionSteps);
    ArgumentNullException.ThrowIfNull(removalSlots);

    if (Revision != expectedRevision)
    {
      throw new InvalidOperationException("The leashed section tick snapshot is stale.");
    }

    LeashedEntityRuntimeSnapshot[] expected = expectedEntities.ToArray();
    var expectedBySlot = new Dictionary<int, LeashedEntityRuntimeSnapshot>(expected.Length);
    foreach (LeashedEntityRuntimeSnapshot entity in expected)
    {
      if (!expectedBySlot.TryAdd(entity.Slot, entity))
      {
        throw new ArgumentException("A section tick contains a duplicate leashed slot.",
          nameof(expectedEntities));
      }
      if (!_bySlot.TryGetValue(entity.Slot, out LeashedEntityRuntimeSnapshot? current) ||
          !Equals(current, entity) || !_identityAuthority.IsActive(entity.Handle))
      {
        throw new InvalidOperationException("The leashed section tick snapshot is stale.");
      }
    }

    var removals = new HashSet<int>();
    foreach (int slot in removalSlots)
    {
      if (!removals.Add(slot))
      {
        throw new ArgumentException("A section tick contains a duplicate removal slot.",
          nameof(removalSlots));
      }
      if (!expectedBySlot.TryGetValue(slot, out LeashedEntityRuntimeSnapshot? entity) ||
          entity.Active)
      {
        throw new ArgumentException(
          "Only a captured inactive leashed slot can be removed by a section tick.",
          nameof(removalSlots));
      }
      if ((_slotByAnchor.TryGetValue(entity.Anchor, out int anchorSlot) && anchorSlot == slot) ||
          !_slotsBySection.TryGetValue(entity.Section, out List<int>? sectionSlots) ||
          sectionSlots.Count(sectionSlot => sectionSlot == slot) != 1)
      {
        throw new InvalidOperationException("The leashed registry indexes are inconsistent.");
      }
    }

    foreach (KeyValuePair<int, LeashedEntityMotionStep> pair in motionSteps)
    {
      if (!expectedBySlot.TryGetValue(pair.Key, out LeashedEntityRuntimeSnapshot? entity) ||
          !entity.Active || removals.Contains(pair.Key))
      {
        throw new ArgumentException(
          "A motion step must target one captured active leashed slot.",
          nameof(motionSteps));
      }
    }

    var changes = new List<LeashedEntityChangeResult>(motionSteps.Count + removals.Count);
    foreach (LeashedEntityRuntimeSnapshot entity in expected)
    {
      if (removals.Contains(entity.Slot))
      {
        changes.Add(new LeashedEntityChangeResult(
          LeashedEntityChangeKind.Removed, null, entity));
        continue;
      }
      if (motionSteps.TryGetValue(entity.Slot, out LeashedEntityMotionStep? step))
      {
        LeashedEntityChangeResult change = PrepareMotionChange(entity, step);
        if (change.Kind != LeashedEntityChangeKind.NoOp)
        {
          changes.Add(change);
        }
      }
    }

    MechanismEntityHandle[] retiredIdentities = changes
      .Where(change => change.Kind == LeashedEntityChangeKind.Removed)
      .Select(change => change.Previous!.Handle)
      .ToArray();
    if (retiredIdentities.Distinct().Count() != retiredIdentities.Length)
    {
      throw new InvalidOperationException("A section tick contains a duplicate identity.");
    }

    var sectionSlotsAfterRemoval = new Dictionary<MechanismSectionCoordinate, List<int>?>();
    foreach (IGrouping<MechanismSectionCoordinate, LeashedEntityRuntimeSnapshot> group in changes
        .Where(change => change.Kind == LeashedEntityChangeKind.Removed)
        .Select(change => change.Previous!)
        .GroupBy(entity => entity.Section))
    {
      List<int> candidates = _slotsBySection[group.Key].ToList();
      foreach (LeashedEntityRuntimeSnapshot entity in group)
      {
        candidates.Remove(entity.Slot);
      }
      if (candidates.Count == 0)
      {
        sectionSlotsAfterRemoval.Add(group.Key, null);
      }
      else
      {
        sectionSlotsAfterRemoval.Add(group.Key, candidates);
      }
    }

    return new SectionTickPreparation(
      this,
      Array.AsReadOnly(expected),
      Array.AsReadOnly(changes.ToArray()),
      retiredIdentities,
      sectionSlotsAfterRemoval);
  }

  public LeashedEntityChangeResult ApplyFull(
    LeashedEntityMessage message,
    MechanismSectionCoordinate section)
  {
    return ApplyFull(message, section, null);
  }

  private LeashedEntityChangeResult ApplyFull(
    LeashedEntityMessage message,
    MechanismSectionCoordinate section,
    LeashedEntityMotionState? motion)
  {
    ValidateSection(section);
    ValidateMessageHeader(message, expectedAction: 1);
    byte type = (byte)message.EntityType!.Value;
    var anchor = new TileCoordinate(message.AnchorX!.Value, message.AnchorY!.Value);
    ValidateState(type, message.State!, full: true);
    if (motion is not null)
    {
      ValidateMotionProjection(anchor, message.State!, motion);
    }

    if (_bySlot.TryGetValue(message.Slot, out LeashedEntityRuntimeSnapshot? existing))
    {
      if (!existing.Active)
      {
        throw new InvalidOperationException("A full update cannot reactivate a detached leashed slot.");
      }
      if (existing.Type != type || existing.Anchor != anchor)
      {
        throw new InvalidOperationException(
          "A full update cannot change an occupied leashed slot's type or anchor.");
      }
      if (existing.Section != section)
      {
        throw new InvalidOperationException(
          "A full update cannot move an unchanged anchor between sections.");
      }
      EnsureIdentityActive(existing.Handle);
      long nextRevision = checked(Revision + 1);
      LeashedEntityRuntimeSnapshot updated = existing with
      {
        State = message.State!,
        Motion = motion,
        Revision = checked(existing.Revision + 1)
      };
      _bySlot[message.Slot] = updated;
      Revision = nextRevision;
      return new LeashedEntityChangeResult(LeashedEntityChangeKind.Updated, updated, existing);
    }

    if (_slotByAnchor.ContainsKey(anchor))
    {
      throw new InvalidOperationException(
        $"A leashed entity already occupies anchor {anchor.X},{anchor.Y}.");
    }
    long nextRegistryRevision = checked(Revision + 1);
    MechanismEntityHandle identity = _identityFactory();
    if (_bySlot.Values.Any(entity => entity.Handle == identity))
    {
      throw new InvalidOperationException("The identity factory returned an already-owned identity.");
    }
    ValidateNewIdentity(identity);
    var created = new LeashedEntityRuntimeSnapshot(
      identity,
      message.Slot,
      type,
      anchor,
      section,
      message.State!,
      1,
      motion);
    _bySlot.Add(message.Slot, created);
    _slotByAnchor.Add(anchor, message.Slot);
    AddToSection(section, message.Slot);
    Revision = nextRegistryRevision;
    return new LeashedEntityChangeResult(LeashedEntityChangeKind.Created, created, null);
  }

  public LeashedEntityChangeResult Apply(
    LeashedEntityMessage message,
    MechanismSectionCoordinate section)
  {
    ArgumentNullException.ThrowIfNull(message);
    return message.Action switch
    {
      0 => ApplyRemovePacket(message),
      1 => ApplyFull(message, section),
      2 => ApplyPartial(message),
      _ => throw new ArgumentOutOfRangeException(nameof(message),
        "The Version4 leashed action must be 0, 1 or 2.")
    };
  }

  public LeashedEntityChangeResult ApplyPartial(LeashedEntityMessage message)
  {
    return ApplyPartial(message, null);
  }

  public LeashedEntityChangeResult ApplyMotion(
    LeashedEntityRuntimeSnapshot expected,
    LeashedEntityMotionStep step)
  {
    ArgumentNullException.ThrowIfNull(expected);
    ArgumentNullException.ThrowIfNull(step);
    if (!_bySlot.TryGetValue(expected.Slot, out LeashedEntityRuntimeSnapshot? current) ||
        current.Handle != expected.Handle || current.Revision != expected.Revision || !current.Active)
    {
      throw new InvalidOperationException("The leashed motion snapshot is stale.");
    }
    LeashedEntityChangeResult change = PrepareMotionChange(current, step);
    if (change.Kind == LeashedEntityChangeKind.Updated)
    {
      _bySlot[change.Current!.Slot] = change.Current;
      Revision = checked(Revision + 1);
    }
    return change;
  }

  private LeashedEntityChangeResult PrepareMotionChange(
    LeashedEntityRuntimeSnapshot existing,
    LeashedEntityMotionStep step)
  {
    ValidateMotion(step.Motion);
    if (existing.Motion is not null && step.Motion.LastTick <= existing.Motion.LastTick)
    {
      throw new ArgumentException("The leashed motion is invalid or already committed.", nameof(step));
    }
    if (!existing.Active)
    {
      throw new InvalidOperationException("A partial update cannot advance an inactive leashed slot.");
    }
    EnsureIdentityActive(existing.Handle);
    ValidateState(existing.Type, step.PartialState, full: false);
    LeashedEntityState mergedState = Merge(existing.State, step.PartialState);
    ValidateMotionProjection(existing.Anchor, mergedState, step.Motion);
    if (Equals(mergedState, existing.State) && Equals(step.Motion, existing.Motion))
    {
      return new LeashedEntityChangeResult(LeashedEntityChangeKind.NoOp, null, null);
    }
    LeashedEntityRuntimeSnapshot updated = existing with
    {
      State = mergedState,
      Motion = step.Motion,
      Revision = checked(existing.Revision + 1)
    };
    return new LeashedEntityChangeResult(
      LeashedEntityChangeKind.Updated, updated, existing);
  }

  private LeashedEntityChangeResult ApplyPartial(
    LeashedEntityMessage message,
    LeashedEntityMotionState? motion)
  {
    ValidateMessageHeader(message, expectedAction: 2);
    if (!_bySlot.TryGetValue(message.Slot, out LeashedEntityRuntimeSnapshot? existing))
    {
      throw new InvalidOperationException("A partial update cannot create a leashed entity.");
    }
    if (!existing.Active)
    {
      throw new InvalidOperationException("A partial update cannot advance an inactive leashed slot.");
    }
    byte type = (byte)message.EntityType!.Value;
    if (existing.Type != type)
    {
      throw new InvalidOperationException("A partial update's type does not match its slot.");
    }
    EnsureIdentityActive(existing.Handle);
    ValidateState(type, message.State!, full: false);
    LeashedEntityState mergedState = Merge(existing.State, message.State!);
    if (motion is not null)
    {
      ValidateMotionProjection(existing.Anchor, mergedState, motion);
    }
    if (Equals(mergedState, existing.State) && Equals(motion, existing.Motion))
    {
      return new LeashedEntityChangeResult(LeashedEntityChangeKind.NoOp, null, null);
    }
    long nextRegistryRevision = checked(Revision + 1);
    LeashedEntityRuntimeSnapshot updated = existing with
    {
      State = mergedState,
      Motion = motion,
      Revision = checked(existing.Revision + 1)
    };
    _bySlot[message.Slot] = updated;
    Revision = nextRegistryRevision;
    return new LeashedEntityChangeResult(LeashedEntityChangeKind.Updated, updated, existing);
  }

  public LeashedEntityChangeResult Remove(int slot)
  {
    if (slot < 0 || slot >= _maximumSlots)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
    if (!_bySlot.TryGetValue(slot, out LeashedEntityRuntimeSnapshot? existing))
    {
      return new LeashedEntityChangeResult(LeashedEntityChangeKind.NoOp, null, null);
    }
    bool ownsAnchor = _slotByAnchor.TryGetValue(existing.Anchor, out int indexedSlot) &&
      indexedSlot == slot;
    if ((existing.Active && !ownsAnchor) || (!existing.Active && ownsAnchor) ||
        !_slotsBySection.TryGetValue(existing.Section, out List<int>? sectionSlots) ||
        !sectionSlots.Contains(slot))
    {
      throw new InvalidOperationException("The leashed registry indexes are inconsistent.");
    }
    EnsureIdentityActive(existing.Handle);
    long nextRevision = checked(Revision + 1);
    var result = new LeashedEntityChangeResult(LeashedEntityChangeKind.Removed, null, existing);
    if (!_identityAuthority.TryRetireMany([existing.Handle]))
    {
      throw new InvalidOperationException("The leashed identity could not be retired.");
    }
    _bySlot.Remove(slot);
    if (ownsAnchor)
    {
      _slotByAnchor.Remove(existing.Anchor);
    }
    RemoveFromSection(existing.Section, slot);
    Revision = nextRevision;
    return result;
  }

  public void ClearForWorldUnload()
  {
    if (_bySlot.Count == 0)
    {
      return;
    }
    long nextRevision = checked(Revision + 1);
    MechanismEntityHandle[] identities = _bySlot.Values
      .Select(entity => entity.Handle)
      .ToArray();
    if (!_identityAuthority.TryRetireMany(identities))
    {
      throw new InvalidOperationException("Leashed identities could not be retired for unload.");
    }
    _bySlot.Clear();
    _slotByAnchor.Clear();
    _slotsBySection.Clear();
    Revision = nextRevision;
  }

  private void ValidateMessageHeader(LeashedEntityMessage message, byte expectedAction)
  {
    ArgumentNullException.ThrowIfNull(message);
    if (message.Action != expectedAction || message.Slot < 0 || message.Slot >= _maximumSlots)
    {
      throw new ArgumentOutOfRangeException(nameof(message),
        "The leashed packet action or slot is invalid for this registry.");
    }
    if (message.EntityType is not int type || !_facts.LeashedPrototypes.ContainsKey((byte)type) ||
        type is < 1 or > 19)
    {
      throw new ArgumentException("The leashed packet uses an unknown prototype type.", nameof(message));
    }
    if (expectedAction == 1 &&
        (message.AnchorX is not short x || message.AnchorY is not short y || x < 0 || y < 0))
    {
      throw new ArgumentException("A full leashed packet requires nonnegative anchor coordinates.",
        nameof(message));
    }
    if (expectedAction == 2 && (message.AnchorX is not null || message.AnchorY is not null))
    {
      throw new ArgumentException("A partial leashed packet cannot carry anchor coordinates.",
        nameof(message));
    }
    if (message.State is null)
    {
      throw new ArgumentException(
        "A full or partial leashed packet requires a typed state.",
        nameof(message));
    }
  }

  private void ValidateNewIdentity(MechanismEntityHandle identity)
  {
    if (!identity.IsValid || identity.WorldScope != _worldScope ||
        !_identityAuthority.IsActive(identity) ||
        _bySlot.Values.Any(entity => entity.Handle == identity))
    {
      throw new InvalidOperationException(
        "The identity factory must return a unique, active identity in this world scope.");
    }
  }

  private void EnsureIdentityActive(MechanismEntityHandle identity)
  {
    if (identity.WorldScope != _worldScope || !_identityAuthority.IsActive(identity))
    {
      throw new InvalidOperationException("The leashed slot contains a stale identity.");
    }
  }

  private void ValidateState(byte type, LeashedEntityState state, bool full)
  {
    LeashedPrototypeDefinition definition = _facts.GetLeashedPrototype(type);
    if (definition.Kind == LeashedPrototypeKind.Kite && state is KiteLeashedEntityState kite)
    {
      bool projectileTypeValid = full
        ? kite.ProjectileType is >= 0
        : kite.ProjectileType is null;
      if (!projectileTypeValid ||
          !float.IsFinite(kite.PositionX) || !float.IsFinite(kite.PositionY) ||
          !float.IsFinite(kite.WindTarget) || !float.IsFinite(kite.CloudAlpha) ||
          !float.IsFinite(kite.TimeCounter) || !LeashedPackedVector.IsFinite(kite.PackedVelocity))
      {
        throw new ArgumentException("The kite state does not match its Full/Partial shape.", nameof(state));
      }
      return;
    }
    if (definition.Kind != LeashedPrototypeKind.Kite && state is CritterLeashedEntityState critter)
    {
      bool coreFieldsValid = full
        ? critter.NpcType is >= 0 &&
          critter.SizeX is float sizeX && float.IsFinite(sizeX) &&
          sizeX >= 1 && sizeX < int.MaxValue &&
          critter.SizeY is float sizeY && float.IsFinite(sizeY) &&
          sizeY >= 1 && sizeY < int.MaxValue
        : critter.NpcType is null && critter.SizeX is null && critter.SizeY is null;
      bool subtypeFieldsValid = type switch
      {
        7 => full
          ? critter.ButterflyVariant is not null && critter.OldPositionsLength is null
          : critter.ButterflyVariant is null && critter.OldPositionsLength is null,
        11 => full
          ? critter.OldPositionsLength is not null && critter.ButterflyVariant is null
          : critter.OldPositionsLength is null && critter.ButterflyVariant is null,
        _ => critter.ButterflyVariant is null && critter.OldPositionsLength is null
      };
      if (!coreFieldsValid || !subtypeFieldsValid ||
          !LeashedPackedVector.IsFinite(critter.PackedRelativePosition))
      {
        throw new ArgumentException("The critter state does not match its Full/Partial shape.", nameof(state));
      }
      return;
    }
    throw new ArgumentException("The leashed state does not match its prototype type.", nameof(state));
  }

  private static void ValidateMotion(LeashedEntityMotionState motion)
  {
    ArgumentNullException.ThrowIfNull(motion);
    if (!float.IsFinite(motion.Position.X) || !float.IsFinite(motion.Position.Y) ||
        !float.IsFinite(motion.Velocity.X) || !float.IsFinite(motion.Velocity.Y) ||
        motion.SpriteDirection is < -1 or > 1 || motion.Direction is < -1 or > 1 ||
        motion.LastTick < 0)
    {
      throw new ArgumentException("The leashed runtime motion is invalid.", nameof(motion));
    }
  }

  private static void ValidateMotionProjection(
    TileCoordinate anchor,
    LeashedEntityState state,
    LeashedEntityMotionState motion)
  {
    ValidateMotion(motion);
    bool matches = state switch
    {
      CritterLeashedEntityState critter =>
        (motion.Direction is null || critter.DirectionPositive == (motion.Direction > 0)) &&
        critter.PackedRelativePosition == LeashedPackedVector.Pack(
          motion.Position - new Vector2(anchor.X * 16 + 8, anchor.Y * 16 + 8)),
      KiteLeashedEntityState kite => kite.PositionX == motion.Position.X &&
        kite.PositionY == motion.Position.Y &&
        kite.PackedVelocity == LeashedPackedVector.Pack(motion.Velocity),
      _ => false
    };
    if (!matches)
    {
      throw new ArgumentException("The wire projection does not match the runtime motion.",
        nameof(state));
    }
  }

  private static LeashedEntityState Merge(LeashedEntityState previous, LeashedEntityState update)
  {
    if (previous is KiteLeashedEntityState oldKite && update is KiteLeashedEntityState newKite)
    {
      return newKite with { ProjectileType = oldKite.ProjectileType };
    }
    if (previous is CritterLeashedEntityState oldCritter &&
        update is CritterLeashedEntityState newCritter)
    {
      return newCritter with
      {
        NpcType = oldCritter.NpcType,
        SizeX = oldCritter.SizeX,
        SizeY = oldCritter.SizeY,
        ButterflyVariant = oldCritter.ButterflyVariant,
        OldPositionsLength = oldCritter.OldPositionsLength
      };
    }
    throw new InvalidOperationException("A leashed partial state cannot merge with its current type.");
  }

  private void AddToSection(MechanismSectionCoordinate section, int slot)
  {
    if (!_slotsBySection.TryGetValue(section, out List<int>? slots))
    {
      slots = [];
      _slotsBySection.Add(section, slots);
    }
    if (slots.Contains(slot))
    {
      throw new InvalidOperationException("The leashed section index contains a duplicate slot.");
    }
    slots.Add(slot);
  }

  private LeashedEntityChangeResult ApplyRemovePacket(LeashedEntityMessage message)
  {
    if (message.Slot < 0 || message.Slot >= _maximumSlots ||
        message.EntityType is not null || message.AnchorX is not null ||
        message.AnchorY is not null || message.State is not null)
    {
      throw new ArgumentException("A remove message must contain only an in-range slot.",
        nameof(message));
    }
    return Remove(message.Slot);
  }

  private void RemoveFromSection(MechanismSectionCoordinate section, int slot)
  {
    if (!_slotsBySection.TryGetValue(section, out List<int>? slots) || !slots.Remove(slot))
    {
      throw new InvalidOperationException("The leashed section index is inconsistent.");
    }
    if (slots.Count == 0)
    {
      _slotsBySection.Remove(section);
    }
  }

  private static void ValidateSection(MechanismSectionCoordinate section)
  {
    if (section.X < 0 || section.Y < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(section));
    }
  }
}
