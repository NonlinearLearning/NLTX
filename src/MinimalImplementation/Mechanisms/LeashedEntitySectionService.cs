namespace MinimalImplementation.Server.Mechanisms;

/// <summary>
/// Controls active leashed sections without owning prototype-specific motion rules.
/// </summary>
/// <remarks>
/// This service is synchronous and is intended to run on the single domain commit thread.
/// Deactivation retains state and identities; Version4 prototypes inherit the empty Despawn hook.
/// Activation prepares Spawn(false) for every retained entity before publishing the section.
/// The returned full and partial projections are effects for the host to publish after commit.
/// The host owns observer membership: activate on its first observer, capture the current Full
/// baseline for each later observer, and deactivate after its last observer leaves. Repeating
/// activation does not return an observer baseline. The host also settles transport receipts.
/// </remarks>
public sealed class LeashedEntitySectionService
{
  public const int PeriodicUpdateIntervalTicks = 1 << 10;

  private const long PeriodicUpdateMask = PeriodicUpdateIntervalTicks - 1;

  private readonly LeashedEntityRegistry _registry;
  private readonly HashSet<MechanismSectionCoordinate> _activeSections = [];
  private readonly List<MechanismSectionCoordinate> _activationOrder = [];
  private long _activeSectionsRevision;

  public LeashedEntitySectionService(LeashedEntityRegistry registry)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
  }

  public IReadOnlyList<MechanismSectionCoordinate> ActiveSections =>
    Array.AsReadOnly(_activeSections
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray());

  public bool IsActive(MechanismSectionCoordinate section)
  {
    ValidateSection(section);
    return _activeSections.Contains(section);
  }

  /// <summary>
  /// Activates an empty section. Retained entities require the explicit spawn overload.
  /// </summary>
  public SectionActivationResult ActivateSection(MechanismSectionCoordinate section)
  {
    ValidateSection(section);
    if (_activeSections.Contains(section))
    {
      return new SectionActivationResult(
        section,
        WasActivated: false,
        FullEntities: Array.Empty<LeashedEntityRuntimeSnapshot>());
    }
    if (_registry.CaptureSection(section).Count != 0)
    {
      throw new InvalidOperationException(
        "A retained section requires Spawn(false) with authoritative seeds and environment.");
    }
    return ActivateSection(section, 0, static (_, _) =>
      throw new InvalidOperationException("An empty section cannot contain a spawn candidate."));
  }

  /// <summary>
  /// The spawn callback prepares state only. Its seed/environment reads are explicit host effects;
  /// it must not change section or registry state or publish packets. Failed preparation consumes
  /// no section state.
  /// </summary>
  public SectionActivationResult ActivateSection(
    MechanismSectionCoordinate section,
    long tick,
    Func<LeashedEntityRuntimeSnapshot, long, LeashedEntitySpawnState> prepareSpawn)
  {
    return PrepareActivateSection(section, tick, prepareSpawn).Commit();
  }

  /// <summary>
  /// Prepares respawn states and the Full baseline without activating the section.
  /// </summary>
  /// <remarks>
  /// The host may encode and validate Result before committing in the same exclusive owner window.
  /// The callback must not mutate section or registry state or publish effects.
  /// Seed/environment reads belong to the callback and are not undone by discarding a preparation.
  /// Candidate Full projections must not be published until Commit succeeds.
  /// </remarks>
  public SectionActivationPreparation PrepareActivateSection(
    MechanismSectionCoordinate section,
    long tick,
    Func<LeashedEntityRuntimeSnapshot, long, LeashedEntitySpawnState> prepareSpawn)
  {
    ValidateSection(section);
    ArgumentOutOfRangeException.ThrowIfNegative(tick);
    ArgumentNullException.ThrowIfNull(prepareSpawn);
    if (_activeSections.Contains(section))
    {
      return new SectionActivationPreparation(this, _activeSectionsRevision, null,
        new SectionActivationResult(section, false,
          Array.Empty<LeashedEntityRuntimeSnapshot>()));
    }
    long expectedActiveSectionsRevision = _activeSectionsRevision;
    _ = checked(expectedActiveSectionsRevision + 1);
    long expectedRevision = _registry.Revision;
    var spawns = new Dictionary<int, LeashedEntitySpawnState>();
    foreach (LeashedEntityRuntimeSnapshot entity in _registry.CaptureSection(section))
    {
      if (entity.Active)
      {
        LeashedEntitySpawnState spawn = prepareSpawn.Invoke(entity, tick);
        if (spawn?.Motion is null || spawn.Motion.LastTick != tick)
        {
          throw new InvalidOperationException(
            "A leashed section spawn must carry motion for the activation tick.");
        }
        spawns.Add(entity.Slot, spawn);
      }
    }
    LeashedEntityRegistry.SectionRespawnPreparation preparation =
      _registry.PrepareSectionRespawn(section, expectedRevision, spawns);
    LeashedEntityRuntimeSnapshot[] activeEntities = preparation.Entities
      .Where(entity => entity.Active)
      .ToArray();
    var result = new SectionActivationResult(
      section, true, Array.AsReadOnly(activeEntities));
    _activeSections.EnsureCapacity(_activeSections.Count + 1);
    _activationOrder.EnsureCapacity(_activationOrder.Count + 1);
    var sectionPreparation = new SectionActivationPreparation(
      this, expectedActiveSectionsRevision, preparation, result);
    if (!sectionPreparation.Validate())
    {
      throw new InvalidOperationException("The leashed section respawn became stale.");
    }
    return sectionPreparation;
  }

  /// <summary>
  /// Deactivates a section while retaining its registry entities and identities.
  /// Captures and validates retained entities before changing active-section state.
  /// </summary>
  public SectionDeactivationResult DeactivateSection(MechanismSectionCoordinate section)
  {
    ValidateSection(section);
    IReadOnlyList<LeashedEntityRuntimeSnapshot> retainedEntities =
      _registry.CaptureSection(section);
    bool wasDeactivated = _activeSections.Contains(section);
    var result = new SectionDeactivationResult(section, wasDeactivated, retainedEntities);
    if (wasDeactivated)
    {
      long nextActiveSectionsRevision = checked(_activeSectionsRevision + 1);
      _activeSections.Remove(section);
      _activationOrder.Remove(section);
      _activeSectionsRevision = nextActiveSectionsRevision;
    }
    return result;
  }

  /// <summary>
  /// Returns the current section snapshot for a new observer without changing active state.
  /// </summary>
  /// <remarks>
  /// Later observers of an active section use this snapshot without respawning or consuming seeds.
  /// The host must encode the entire Full group and validate its owner window before admitting
  /// that observer. This capture neither registers an observer nor acknowledges packet delivery.
  /// </remarks>
  public IReadOnlyList<LeashedEntityRuntimeSnapshot> CaptureForObserver(
    MechanismSectionCoordinate section)
  {
    ValidateSection(section);
    return Array.AsReadOnly(_registry.CaptureSection(section)
      .Where(entity => entity.Active)
      .ToArray());
  }

  /// <summary>
  /// Retires all leashed identities and clears active section state for world unload.
  /// </summary>
  public WorldUnloadResult ClearForWorldUnload()
  {
    long nextActiveSectionsRevision = checked(_activeSectionsRevision + 1);
    int retiredEntityCount = _registry.Count;
    int clearedSectionCount = _activeSections.Count;
    _registry.ClearForWorldUnload();
    _activeSections.Clear();
    _activationOrder.Clear();
    _activeSectionsRevision = nextActiveSectionsRevision;
    return new WorldUnloadResult(retiredEntityCount, clearedSectionCount);
  }

  /// <summary>
  /// Advances only entities in active sections and schedules periodic Partial projections.
  /// </summary>
  public SectionTickResult AdvanceActiveSections(
    long tick,
    ILeashedEntityMotionPort motionPort)
  {
    return PrepareAdvanceActiveSections(tick, motionPort).Commit();
  }

  /// <summary>
  /// Prepares motion, removals and periodic projections without committing registry state.
  /// </summary>
  /// <remarks>
  /// The host may encode and validate Result before committing in the same exclusive owner window.
  /// Motion/environment reads belong to the supplied port; it must not mutate section or registry
  /// state or publish effects. Discarding this preparation does not undo the port's external reads.
  /// </remarks>
  public SectionTickPreparation PrepareAdvanceActiveSections(
    long tick,
    ILeashedEntityMotionPort motionPort)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }
    ArgumentNullException.ThrowIfNull(motionPort);

    var expectedEntities = new List<LeashedEntityRuntimeSnapshot>();
    var motionSteps = new Dictionary<int, LeashedEntityMotionStep>();
    var removalSlots = new List<int>();
    var visitedSlots = new HashSet<int>();
    long expectedActiveSectionsRevision = _activeSectionsRevision;
    long expectedRevision = _registry.Revision;
    foreach (MechanismSectionCoordinate section in _activationOrder)
    {
      foreach (LeashedEntityRuntimeSnapshot before in _registry.CaptureSection(section))
      {
        if (!visitedSlots.Add(before.Slot))
        {
          throw new InvalidOperationException(
            "An active leashed section yielded the same slot more than once.");
        }

        expectedEntities.Add(before);
        if (!before.Active)
        {
          removalSlots.Add(before.Slot);
          continue;
        }

        bool advanced = motionPort.TryAdvance(before, tick, out LeashedEntityMotionStep? step);
        if (advanced)
        {
          if (step is null || step.Motion.LastTick != tick)
          {
            throw new InvalidOperationException(
              "A leashed motion step must carry a Partial state for the current game tick.");
          }
          motionSteps.Add(before.Slot, step);
        }
        else if (step is not null)
        {
          throw new InvalidOperationException(
            "A leashed motion port returned a step without reporting advancement.");
        }
      }
    }

    LeashedEntityRegistry.SectionTickPreparation preparation =
      _registry.PrepareSectionTick(
        expectedRevision, expectedEntities, motionSteps, removalSlots);
    IReadOnlyList<LeashedEntityChangeResult> stateChanges = preparation.Changes;
    var changesBySlot = stateChanges.ToDictionary(change =>
      (change.Current ?? change.Previous!).Slot);
    var periodicUpdates = new List<PeriodicPartialUpdate>();
    foreach (LeashedEntityRuntimeSnapshot before in expectedEntities)
    {
      if (!before.Active || !IsPeriodicUpdateDue(tick, before.Slot))
      {
        continue;
      }

      if (changesBySlot.TryGetValue(before.Slot, out LeashedEntityChangeResult? change) &&
          change.Kind == LeashedEntityChangeKind.Updated)
      {
        periodicUpdates.Add(new PeriodicPartialUpdate(change.Current!, change.Previous!));
      }
      else
      {
        periodicUpdates.Add(new PeriodicPartialUpdate(before, before));
      }
    }

    var result = new SectionTickResult(
      tick,
      stateChanges,
      Array.AsReadOnly(periodicUpdates.ToArray()));
    var sectionPreparation = new SectionTickPreparation(
      this, expectedActiveSectionsRevision, preparation, result);
    if (!sectionPreparation.Validate())
    {
      throw new InvalidOperationException("The leashed section tick became stale.");
    }
    return sectionPreparation;
  }

  private static bool IsPeriodicUpdateDue(long tick, int slot)
  {
    long normalizedTick = tick & PeriodicUpdateMask;
    long normalizedSlot = slot & PeriodicUpdateMask;
    return ((normalizedTick + normalizedSlot) & PeriodicUpdateMask) == 0;
  }

  private static void ValidateSection(MechanismSectionCoordinate section)
  {
    if (section.X < 0 || section.Y < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(section));
    }
  }

  /// <summary>Candidate Full effects remain tentative until Commit succeeds.</summary>
  public sealed class SectionActivationPreparation
  {
    private readonly LeashedEntitySectionService _owner;
    private readonly long _activeSectionsRevision;
    private readonly long _registryRevision;
    private readonly LeashedEntityRegistry.SectionRespawnPreparation? _registryPreparation;
    private bool _committed;

    public SectionActivationResult Result { get; }

    internal SectionActivationPreparation(
      LeashedEntitySectionService owner,
      long activeSectionsRevision,
      LeashedEntityRegistry.SectionRespawnPreparation? registryPreparation,
      SectionActivationResult result)
    {
      _owner = owner;
      _activeSectionsRevision = activeSectionsRevision;
      _registryRevision = owner._registry.Revision;
      _registryPreparation = registryPreparation;
      Result = result;
    }

    public bool Validate()
    {
      return !_committed && _owner._activeSectionsRevision == _activeSectionsRevision &&
        _owner._registry.Revision == _registryRevision &&
        (_registryPreparation is null || _registryPreparation.Validate());
    }

    public SectionActivationResult Commit()
    {
      if (!Validate())
      {
        throw new InvalidOperationException("The leashed section respawn became stale.");
      }
      if (_registryPreparation is not null)
      {
        long nextActiveSectionsRevision = checked(_activeSectionsRevision + 1);
        _registryPreparation.Commit();
        _owner._activeSections.Add(Result.Section);
        _owner._activationOrder.Add(Result.Section);
        _owner._activeSectionsRevision = nextActiveSectionsRevision;
      }
      _committed = true;
      return Result;
    }
  }

  /// <summary>Candidate tick effects remain tentative until Commit succeeds.</summary>
  public sealed class SectionTickPreparation
  {
    private readonly LeashedEntitySectionService _owner;
    private readonly long _activeSectionsRevision;
    private readonly LeashedEntityRegistry.SectionTickPreparation _registryPreparation;

    public SectionTickResult Result { get; }

    internal SectionTickPreparation(
      LeashedEntitySectionService owner,
      long activeSectionsRevision,
      LeashedEntityRegistry.SectionTickPreparation registryPreparation,
      SectionTickResult result)
    {
      _owner = owner;
      _activeSectionsRevision = activeSectionsRevision;
      _registryPreparation = registryPreparation;
      Result = result;
    }

    public bool Validate()
    {
      return _owner._activeSectionsRevision == _activeSectionsRevision &&
        _registryPreparation.Validate();
    }

    /// <summary>
    /// Revalidates section and registry state, then retires identities and commits prepared changes.
    /// Identity retirement can still reject the commit; the host must publish effects only on success.
    /// </summary>
    public SectionTickResult Commit()
    {
      if (!Validate())
      {
        throw new InvalidOperationException("The leashed section tick became stale.");
      }
      _registryPreparation.Commit();
      return Result;
    }
  }

  public sealed record SectionActivationResult(
    MechanismSectionCoordinate Section,
    bool WasActivated,
    IReadOnlyList<LeashedEntityRuntimeSnapshot> FullEntities);

  public sealed record SectionDeactivationResult(
    MechanismSectionCoordinate Section,
    bool WasDeactivated,
    IReadOnlyList<LeashedEntityRuntimeSnapshot> RetainedEntities);

  public sealed record PeriodicPartialUpdate(
    LeashedEntityRuntimeSnapshot Current,
    LeashedEntityRuntimeSnapshot Previous);

  public sealed record WorldUnloadResult(
    int RetiredEntityCount,
    int ClearedSectionCount);

  public sealed record SectionTickResult(
    long Tick,
    IReadOnlyList<LeashedEntityChangeResult> StateChanges,
    IReadOnlyList<PeriodicPartialUpdate> PeriodicUpdates);
}
