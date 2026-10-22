using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>Tracks the ordered baseline state of one section for one world session.</summary>
public enum WorldSectionTransferState
{
  NotScheduled,
  Captured,
  Queued,
  LocallySent
}

/// <summary>Stable result categories for section planning, delivery and retry boundaries.</summary>
public enum WorldSectionTransferStatus
{
  Accepted,
  Captured,
  Queued,
  LocallySent,
  NotSubmitted,
  OutcomeUnknown,
  TransportClosed,
  AlreadyScheduled,
  DuplicateRequest,
  InvalidInput,
  InvalidSession,
  SessionInvalidated,
  StaleWorld,
  StalePlan,
  StaleVersion,
  InvalidSection,
  BudgetExceeded,
  NotScheduled,
  BaselinePending,
  NotCaptured,
  InvalidTransition,
  FactsUnavailable
}

/// <summary>Outcome of the complete packet-10 fragment group for one section.</summary>
public enum WorldSectionTransferSendOutcome
{
  NotSubmitted,
  Queued,
  LocallySent,
  OutcomeUnknown,
  TransportClosed
}

/// <summary>
/// Binds a transfer to the authenticated session, the published world instance and its plan
/// version. The transfer ID is supplied by the session owner so this boundary does not create a
/// hidden retry identity.
/// </summary>
public readonly record struct WorldSectionTransferToken(
  SessionEpoch SessionEpoch,
  Guid TransferId,
  ulong WorldEpoch,
  CommitVersion PlanVersion)
{
  public bool IsValid =>
    SessionEpoch.IsValid && TransferId != Guid.Empty && WorldEpoch != 0 && PlanVersion.IsValid;
}

/// <summary>Bounds section request and pending-baseline memory for one transfer coordinator.</summary>
public sealed class WorldSectionTransferOptions
{
  public WorldSectionTransferOptions(
    int maximumPendingSections = 128,
    int maximumRequestBatchSections = 32)
  {
    if (maximumPendingSections <= 0 || maximumRequestBatchSections <= 0 ||
        maximumRequestBatchSections > maximumPendingSections)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumPendingSections));
    }

    MaximumPendingSections = maximumPendingSections;
    MaximumRequestBatchSections = maximumRequestBatchSections;
  }

  public int MaximumPendingSections { get; }

  public int MaximumRequestBatchSections { get; }
}

/// <summary>Result of creating a version-bound section transfer session.</summary>
public sealed class WorldSectionTransferStartResult
{
  private WorldSectionTransferStartResult(
    WorldSectionTransferStatus status,
    WorldSectionTransferToken token,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    Status = status;
    Token = token;
    Sections = Array.AsReadOnly(sections
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray());
  }

  public WorldSectionTransferStatus Status { get; }

  public bool Accepted => Status == WorldSectionTransferStatus.Accepted;

  public WorldSectionTransferToken Token { get; }

  public IReadOnlyList<WorldSectionCoordinate> Sections { get; }

  internal static WorldSectionTransferStartResult Success(
    WorldSectionTransferToken token,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    return new WorldSectionTransferStartResult(
      WorldSectionTransferStatus.Accepted,
      token,
      sections);
  }

  internal static WorldSectionTransferStartResult Rejected(WorldSectionTransferStatus status)
  {
    return new WorldSectionTransferStartResult(
      status,
      default,
      Array.Empty<WorldSectionCoordinate>());
  }
}

/// <summary>Result of capturing one or more immutable section baselines.</summary>
public sealed class WorldSectionTransferCaptureResult
{
  private WorldSectionTransferCaptureResult(
    WorldSectionTransferStatus status,
    IReadOnlyCollection<WorldSectionSnapshot> snapshots)
  {
    Status = status;
    Snapshots = Array.AsReadOnly(snapshots
      .OrderBy(snapshot => snapshot.Coordinate.Y)
      .ThenBy(snapshot => snapshot.Coordinate.X)
      .ToArray());
    CommonVersion = Snapshots.Count == 0
      ? null
      : Snapshots.Select(snapshot => snapshot.Version).Distinct().Count() == 1
        ? Snapshots[0].Version
        : null;
  }

  public WorldSectionTransferStatus Status { get; }

  public IReadOnlyList<WorldSectionSnapshot> Snapshots { get; }

  public CommitVersion? CommonVersion { get; }

  internal static WorldSectionTransferCaptureResult Success(
    WorldSectionTransferStatus status,
    IReadOnlyCollection<WorldSectionSnapshot> snapshots)
  {
    return new WorldSectionTransferCaptureResult(status, snapshots);
  }

  internal static WorldSectionTransferCaptureResult Rejected(WorldSectionTransferStatus status)
  {
    return new WorldSectionTransferCaptureResult(status, Array.Empty<WorldSectionSnapshot>());
  }
}

/// <summary>Result of placing complete baselines into the ordered outbound queue.</summary>
public sealed class WorldSectionTransferQueueResult
{
  private WorldSectionTransferQueueResult(
    WorldSectionTransferStatus status,
    IReadOnlyCollection<WorldSectionSnapshot> snapshots)
  {
    Status = status;
    Snapshots = Array.AsReadOnly(snapshots
      .OrderBy(snapshot => snapshot.Coordinate.Y)
      .ThenBy(snapshot => snapshot.Coordinate.X)
      .ToArray());
  }

  public WorldSectionTransferStatus Status { get; }

  public IReadOnlyList<WorldSectionSnapshot> Snapshots { get; }

  internal static WorldSectionTransferQueueResult Success(
    WorldSectionTransferStatus status,
    IReadOnlyCollection<WorldSectionSnapshot> snapshots)
  {
    return new WorldSectionTransferQueueResult(status, snapshots);
  }

  internal static WorldSectionTransferQueueResult Rejected(WorldSectionTransferStatus status)
  {
    return new WorldSectionTransferQueueResult(status, Array.Empty<WorldSectionSnapshot>());
  }
}

/// <summary>Result of one or more versioned liquid-delta section barriers.</summary>
public sealed class WorldLiquidDeltaQueueResult
{
  private WorldLiquidDeltaQueueResult(
    WorldSectionTransferStatus status,
    CommitVersion? version,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    Status = status;
    Version = version;
    Sections = Array.AsReadOnly(sections
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray());
  }

  public WorldSectionTransferStatus Status { get; }

  public CommitVersion? Version { get; }

  public IReadOnlyList<WorldSectionCoordinate> Sections { get; }

  internal static WorldLiquidDeltaQueueResult Success(
    CommitVersion version,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    return new WorldLiquidDeltaQueueResult(
      WorldSectionTransferStatus.Queued,
      version,
      sections);
  }

  internal static WorldLiquidDeltaQueueResult Rejected(WorldSectionTransferStatus status)
  {
    return new WorldLiquidDeltaQueueResult(status, null, Array.Empty<WorldSectionCoordinate>());
  }
}

/// <summary>Result of the bounded Packet 159 section request boundary.</summary>
public sealed class WorldSection159RequestResult
{
  private WorldSection159RequestResult(
    WorldSectionTransferStatus status,
    WorldSectionCoordinate coordinate,
    WorldSectionTransferState? state)
  {
    Status = status;
    Coordinate = coordinate;
    State = state;
  }

  public WorldSectionTransferStatus Status { get; }

  public WorldSectionCoordinate Coordinate { get; }

  public WorldSectionTransferState? State { get; }

  internal static WorldSection159RequestResult Create(
    WorldSectionTransferStatus status,
    WorldSectionCoordinate coordinate,
    WorldSectionTransferState? state = null)
  {
    return new WorldSection159RequestResult(status, coordinate, state);
  }
}

/// <summary>Atomic result for a Packet 159 request batch.</summary>
public sealed class WorldSection159RequestBatchResult
{
  private WorldSection159RequestBatchResult(
    WorldSectionTransferStatus status,
    IReadOnlyCollection<WorldSectionCoordinate> acceptedSections)
  {
    Status = status;
    AcceptedSections = Array.AsReadOnly(acceptedSections
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray());
  }

  public WorldSectionTransferStatus Status { get; }

  public IReadOnlyList<WorldSectionCoordinate> AcceptedSections { get; }

  internal static WorldSection159RequestBatchResult Success(
    IReadOnlyCollection<WorldSectionCoordinate> acceptedSections,
    WorldSectionTransferStatus status = WorldSectionTransferStatus.Accepted)
  {
    return new WorldSection159RequestBatchResult(status, acceptedSections);
  }

  internal static WorldSection159RequestBatchResult Rejected(WorldSectionTransferStatus status)
  {
    return new WorldSection159RequestBatchResult(status, Array.Empty<WorldSectionCoordinate>());
  }
}

/// <summary>
/// Owns the World-side section plan and visibility barrier. It performs no socket I/O: the Host
/// sender reports the result of each complete packet-10 fragment group through RecordSendOutcome.
/// </summary>
public sealed partial class WorldSectionTransferCoordinator
{
  private readonly object _gate = new();
  private readonly WorldOwner _world;
  private readonly WorldSectionTransferOptions _options;
  private readonly Dictionary<Guid, TransferSession> _sessions = [];
  private long _nextQueueOrder;

  public WorldSectionTransferCoordinator(
    WorldOwner world,
    WorldSectionTransferOptions? options = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    _world = world;
    _options = options ?? new WorldSectionTransferOptions();
  }

  /// <summary>
  /// Starts one plan only when its epoch and commit version still describe the published world.
  /// A session epoch can have only one active transfer token.
  /// </summary>
  public WorldSectionTransferStartResult StartSession(
    SessionEpoch sessionEpoch,
    WorldSectionPlan plan,
    Guid transferId)
  {
    ArgumentNullException.ThrowIfNull(plan);
    lock (_gate)
    {
      if (!sessionEpoch.IsValid || transferId == Guid.Empty ||
          plan.WorldEpoch == 0 || !plan.Version.IsValid || plan.Sections.Count == 0)
      {
        return WorldSectionTransferStartResult.Rejected(WorldSectionTransferStatus.InvalidInput);
      }

      if (plan.Sections.Count > _options.MaximumPendingSections)
      {
        return WorldSectionTransferStartResult.Rejected(WorldSectionTransferStatus.BudgetExceeded);
      }

      if (_sessions.ContainsKey(transferId) || _sessions.Values.Any(
          session => session.Token.SessionEpoch == sessionEpoch && !session.Invalidated))
      {
        return WorldSectionTransferStartResult.Rejected(WorldSectionTransferStatus.InvalidInput);
      }

      if (!TryCaptureCurrentHeader(out WorldHeaderCapture? header))
      {
        return WorldSectionTransferStartResult.Rejected(WorldSectionTransferStatus.StaleWorld);
      }

      if (header!.WorldEpoch != plan.WorldEpoch)
      {
        return WorldSectionTransferStartResult.Rejected(WorldSectionTransferStatus.StaleWorld);
      }

      if (header.Version != plan.Version)
      {
        return WorldSectionTransferStartResult.Rejected(WorldSectionTransferStatus.StalePlan);
      }

      WorldSectionCoordinate[] sections = plan.Sections.ToArray();
      if (sections.Distinct().Count() != sections.Length || sections.Any(
          section => !header.Header.Geometry.ContainsSection(section)))
      {
        return WorldSectionTransferStartResult.Rejected(WorldSectionTransferStatus.InvalidSection);
      }

      var token = new WorldSectionTransferToken(
        sessionEpoch,
        transferId,
        plan.WorldEpoch,
        plan.Version);
      _sessions.Add(transferId, new TransferSession(token, header.Header.Geometry, sections));
      return WorldSectionTransferStartResult.Success(token, sections);
    }
  }

  /// <summary>Captures all sections from the original plan in one owner snapshot boundary.</summary>
  public WorldSectionTransferCaptureResult CaptureInitialBaseline(
    WorldSectionTransferToken token)
  {
    lock (_gate)
    {
      if (!TryGetActiveSession(token, out TransferSession? session, out _,
          out WorldSectionTransferStatus status))
      {
        return WorldSectionTransferCaptureResult.Rejected(status);
      }

      return CaptureBaselineCore(session!, session!.InitialSections);
    }
  }

  /// <summary>
  /// Captures requested baselines from the same owner snapshot boundary. Already captured
  /// snapshots are reused for a retry; a new initial-plan capture must still use PlanVersion.
  /// </summary>
  public WorldSectionTransferCaptureResult CaptureBaseline(
    WorldSectionTransferToken token,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    ArgumentNullException.ThrowIfNull(sections);
    lock (_gate)
    {
      if (!TryGetActiveSession(token, out TransferSession? session, out _,
          out WorldSectionTransferStatus status))
      {
        return WorldSectionTransferCaptureResult.Rejected(status);
      }

      return CaptureBaselineCore(session!, sections);
    }
  }

  /// <summary>Queues only complete, captured baselines; no liquid delta may pass this barrier.</summary>
  public WorldSectionTransferQueueResult QueueBaseline(
    WorldSectionTransferToken token,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    ArgumentNullException.ThrowIfNull(sections);
    lock (_gate)
    {
      if (!TryGetActiveSession(token, out TransferSession? session, out _,
          out WorldSectionTransferStatus status))
      {
        return WorldSectionTransferQueueResult.Rejected(status);
      }

      if (!TryNormalizeSections(
          sections,
          session!.Geometry,
          _options.MaximumRequestBatchSections,
          out WorldSectionCoordinate[] requested,
          out status))
      {
        return WorldSectionTransferQueueResult.Rejected(status);
      }

      var snapshots = new List<WorldSectionSnapshot>(requested.Length);
      foreach (WorldSectionCoordinate coordinate in requested)
      {
        if (!session.Sections.TryGetValue(coordinate, out SectionTransferEntry? entry))
        {
          return WorldSectionTransferQueueResult.Rejected(WorldSectionTransferStatus.NotScheduled);
        }

        if (entry.Snapshot is null || entry.State == WorldSectionTransferState.NotScheduled)
        {
          return WorldSectionTransferQueueResult.Rejected(WorldSectionTransferStatus.NotCaptured);
        }

        snapshots.Add(entry.Snapshot);
      }

      bool queuedAny = false;
      bool locallySentOnly = true;
      foreach (WorldSectionCoordinate coordinate in requested)
      {
        SectionTransferEntry entry = session.Sections[coordinate];
        if (entry.State == WorldSectionTransferState.Captured)
        {
          entry.State = WorldSectionTransferState.Queued;
          entry.QueueOrder = ++_nextQueueOrder;
          queuedAny = true;
        }

        if (entry.State != WorldSectionTransferState.LocallySent)
        {
          locallySentOnly = false;
        }
      }

      WorldSectionTransferStatus resultStatus = locallySentOnly
        ? WorldSectionTransferStatus.LocallySent
        : queuedAny || requested.Length > 0
          ? WorldSectionTransferStatus.Queued
          : WorldSectionTransferStatus.Accepted;
      return WorldSectionTransferQueueResult.Success(resultStatus, snapshots);
    }
  }

  /// <summary>
  /// Queues a liquid delta only after every affected section has entered the baseline queue.
  /// The delta is represented by its latest version; the actual bytes remain P01's concern.
  /// </summary>
  public WorldLiquidDeltaQueueResult QueueLiquidDelta(
    WorldSectionTransferToken token,
    IReadOnlyCollection<WorldSectionCoordinate> sections,
    CommitVersion version)
  {
    ArgumentNullException.ThrowIfNull(sections);
    lock (_gate)
    {
      if (!TryGetActiveSession(token, out TransferSession? session, out _,
          out WorldSectionTransferStatus status))
      {
        return WorldLiquidDeltaQueueResult.Rejected(status);
      }

      if (!version.IsValid)
      {
        return WorldLiquidDeltaQueueResult.Rejected(WorldSectionTransferStatus.InvalidInput);
      }

      if (!TryNormalizeSections(
          sections,
          session!.Geometry,
          _options.MaximumRequestBatchSections,
          out WorldSectionCoordinate[] requested,
          out status))
      {
        return WorldLiquidDeltaQueueResult.Rejected(status);
      }

      foreach (WorldSectionCoordinate coordinate in requested)
      {
        if (!session.Sections.TryGetValue(coordinate, out SectionTransferEntry? entry))
        {
          return WorldLiquidDeltaQueueResult.Rejected(WorldSectionTransferStatus.BaselinePending);
        }

        if (entry.State is WorldSectionTransferState.NotScheduled or
            WorldSectionTransferState.Captured)
        {
          return WorldLiquidDeltaQueueResult.Rejected(WorldSectionTransferStatus.BaselinePending);
        }

        if (entry.Snapshot is null || version.Value < entry.Snapshot.Version.Value)
        {
          return WorldLiquidDeltaQueueResult.Rejected(WorldSectionTransferStatus.StaleVersion);
        }
      }

      foreach (WorldSectionCoordinate coordinate in requested)
      {
        SectionTransferEntry entry = session.Sections[coordinate];
        if (entry.PendingDeltaVersion is null ||
            entry.PendingDeltaVersion.Value.Value < version.Value)
        {
          entry.PendingDeltaVersion = version;
        }
      }

      return WorldLiquidDeltaQueueResult.Success(version, requested);
    }
  }

  /// <summary>
  /// Handles the real Packet 159 wire domain: UInt16 coordinates are widened to non-negative
  /// section coordinates, then checked against the current world and bounded request budget.
  /// </summary>
  public WorldSection159RequestResult HandleRequestSection159(
    WorldSectionTransferToken token,
    ushort sectionX,
    ushort sectionY)
  {
    WorldSectionCoordinate coordinate = new(sectionX, sectionY);
    WorldSection159RequestBatchResult result = HandleRequestSection159Batch(
      token,
      [(sectionX, sectionY)]);
    return WorldSection159RequestResult.Create(
      result.Status,
      coordinate,
      result.AcceptedSections.Contains(coordinate)
        ? WorldSectionTransferState.NotScheduled
        : null);
  }

  /// <summary>
  /// Validates a group atomically. Duplicate coordinates in one wire dispatch and over-budget
  /// batches do not add a partially accepted request to the session.
  /// </summary>
  public WorldSection159RequestBatchResult HandleRequestSection159Batch(
    WorldSectionTransferToken token,
    IReadOnlyList<(ushort SectionX, ushort SectionY)> requests)
  {
    ArgumentNullException.ThrowIfNull(requests);
    lock (_gate)
    {
      if (!TryGetActiveSession(token, out TransferSession? session, out _,
          out WorldSectionTransferStatus status))
      {
        return WorldSection159RequestBatchResult.Rejected(status);
      }

      if (requests.Count == 0 || requests.Count > _options.MaximumRequestBatchSections)
      {
        return WorldSection159RequestBatchResult.Rejected(WorldSectionTransferStatus.BudgetExceeded);
      }

      var coordinates = new WorldSectionCoordinate[requests.Count];
      var unique = new HashSet<WorldSectionCoordinate>();
      for (int index = 0; index < requests.Count; index++)
      {
        coordinates[index] = new WorldSectionCoordinate(
          requests[index].SectionX,
          requests[index].SectionY);
        if (!unique.Add(coordinates[index]))
        {
          return WorldSection159RequestBatchResult.Rejected(
            WorldSectionTransferStatus.DuplicateRequest);
        }

        if (!session!.Geometry.ContainsSection(coordinates[index]))
        {
          return WorldSection159RequestBatchResult.Rejected(
            WorldSectionTransferStatus.InvalidSection);
        }
      }

      int newPendingCount = session!.Sections.Count(pair =>
        pair.Value.State != WorldSectionTransferState.LocallySent &&
        !unique.Contains(pair.Key));
      newPendingCount += coordinates.Count(coordinate =>
        !session!.Sections.TryGetValue(coordinate, out SectionTransferEntry? entry) ||
        entry.State != WorldSectionTransferState.LocallySent);
      if (newPendingCount > _options.MaximumPendingSections)
      {
        return WorldSection159RequestBatchResult.Rejected(WorldSectionTransferStatus.BudgetExceeded);
      }

      bool allAlreadyScheduled = coordinates.All(
        coordinate => session.Sections.ContainsKey(coordinate));
      foreach (WorldSectionCoordinate coordinate in coordinates)
      {
        if (session.Sections.ContainsKey(coordinate))
        {
          continue;
        }

        session.Sections.Add(coordinate, new SectionTransferEntry());
      }

      return WorldSection159RequestBatchResult.Success(
        coordinates,
        allAlreadyScheduled
          ? WorldSectionTransferStatus.AlreadyScheduled
          : WorldSectionTransferStatus.Accepted);
    }
  }

  /// <summary>
  /// Applies the result of a complete baseline send. NotSubmitted keeps the immutable capture for
  /// a bounded retry; an unknown/closed transport invalidates the whole session token.
  /// </summary>
  public WorldSectionTransferStatus RecordSendOutcome(
    WorldSectionTransferToken token,
    WorldSectionCoordinate section,
    WorldSectionTransferSendOutcome outcome)
  {
    lock (_gate)
    {
      if (!TryGetActiveSession(token, out TransferSession? session, out _,
          out WorldSectionTransferStatus status))
      {
        return status;
      }

      if (!session!.Sections.TryGetValue(section, out SectionTransferEntry? entry))
      {
        return WorldSectionTransferStatus.NotScheduled;
      }

      if (outcome is WorldSectionTransferSendOutcome.OutcomeUnknown or
          WorldSectionTransferSendOutcome.TransportClosed)
      {
        session.Invalidated = true;
        return outcome == WorldSectionTransferSendOutcome.OutcomeUnknown
          ? WorldSectionTransferStatus.OutcomeUnknown
          : WorldSectionTransferStatus.TransportClosed;
      }

      switch (outcome)
      {
        case WorldSectionTransferSendOutcome.NotSubmitted:
          if (entry.State == WorldSectionTransferState.LocallySent)
          {
            return WorldSectionTransferStatus.InvalidTransition;
          }

          if (entry.State == WorldSectionTransferState.NotScheduled)
          {
            return WorldSectionTransferStatus.NotCaptured;
          }

          entry.State = WorldSectionTransferState.Captured;
          return WorldSectionTransferStatus.NotSubmitted;

        case WorldSectionTransferSendOutcome.Queued:
          if (entry.State == WorldSectionTransferState.NotScheduled)
          {
            return WorldSectionTransferStatus.NotCaptured;
          }

          if (entry.State == WorldSectionTransferState.LocallySent)
          {
            return WorldSectionTransferStatus.LocallySent;
          }

          entry.State = WorldSectionTransferState.Queued;
          return WorldSectionTransferStatus.Queued;

        case WorldSectionTransferSendOutcome.LocallySent:
          if (entry.State is WorldSectionTransferState.NotScheduled or
              WorldSectionTransferState.Captured)
          {
            return WorldSectionTransferStatus.InvalidTransition;
          }

          entry.State = WorldSectionTransferState.LocallySent;
          return WorldSectionTransferStatus.LocallySent;

        default:
          return WorldSectionTransferStatus.InvalidInput;
      }
    }
  }

  public bool TryGetSectionState(
    WorldSectionTransferToken token,
    WorldSectionCoordinate section,
    out WorldSectionTransferState state)
  {
    lock (_gate)
    {
      state = default;
      if (!TryGetActiveSession(token, out TransferSession? session, out _, out _))
      {
        return false;
      }

      if (!session!.Sections.TryGetValue(section, out SectionTransferEntry? entry))
      {
        return false;
      }

      state = entry.State;
      return true;
    }
  }

  public bool TryGetPendingLiquidDeltaVersion(
    WorldSectionTransferToken token,
    WorldSectionCoordinate section,
    out CommitVersion version)
  {
    lock (_gate)
    {
      version = default;
      if (!TryGetActiveSession(token, out TransferSession? session, out _, out _))
      {
        return false;
      }

      if (!session!.Sections.TryGetValue(section, out SectionTransferEntry? entry) ||
          entry.PendingDeltaVersion is null)
      {
        return false;
      }

      version = entry.PendingDeltaVersion.Value;
      return true;
    }
  }

  public bool TryGetWorldGeometry(
    WorldSectionTransferToken token,
    out WorldGeometry geometry)
  {
    lock (_gate)
    {
      geometry = default;
      if (!TryGetActiveSession(token, out TransferSession? session, out _, out _))
      {
        return false;
      }

      geometry = session!.Geometry;
      return true;
    }
  }

  public bool IsSessionValid(WorldSectionTransferToken token)
  {
    lock (_gate)
    {
      return TryGetActiveSession(token, out _, out _, out _);
    }
  }

  /// <summary>Explicitly releases all pending captures when P02 closes the session.</summary>
  public void RetireSession(WorldSectionTransferToken token)
  {
    if (!token.IsValid)
    {
      return;
    }

    lock (_gate)
    {
      if (_sessions.TryGetValue(token.TransferId, out TransferSession? session) &&
          session.Token == token)
      {
        _sessions.Remove(token.TransferId);
      }
    }
  }

  private WorldSectionTransferCaptureResult CaptureBaselineCore(
    TransferSession session,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    if (!TryNormalizeSections(
        sections,
        session.Geometry,
        _options.MaximumRequestBatchSections,
        out WorldSectionCoordinate[] requested,
        out WorldSectionTransferStatus status))
    {
      return WorldSectionTransferCaptureResult.Rejected(status);
    }

    foreach (WorldSectionCoordinate coordinate in requested)
    {
      if (!session.Sections.ContainsKey(coordinate))
      {
        return WorldSectionTransferCaptureResult.Rejected(WorldSectionTransferStatus.NotScheduled);
      }
    }

    WorldSectionCoordinate[] needsCapture = requested
      .Where(coordinate => session.Sections[coordinate].State ==
        WorldSectionTransferState.NotScheduled)
      .ToArray();
    bool capturesInitialPlan = needsCapture.Any(coordinate =>
      session.InitialSectionSet.Contains(coordinate));
    WorldHeaderCapture? current = null;
    if (capturesInitialPlan &&
        (!TryCaptureCurrentHeader(out current) ||
         current is null ||
         current.WorldEpoch != session.Token.WorldEpoch))
    {
      return WorldSectionTransferCaptureResult.Rejected(WorldSectionTransferStatus.StaleWorld);
    }

    if (capturesInitialPlan && current is not null &&
        current.Version != session.Token.PlanVersion)
    {
      return WorldSectionTransferCaptureResult.Rejected(WorldSectionTransferStatus.StalePlan);
    }

    if (needsCapture.Length > 0)
    {
      WorldSectionSnapshotBatch batch;
      try
      {
        batch = _world.CaptureSections(needsCapture);
      }
      catch (InvalidOperationException)
      {
        return WorldSectionTransferCaptureResult.Rejected(WorldSectionTransferStatus.StaleWorld);
      }
      catch (ArgumentOutOfRangeException)
      {
        return WorldSectionTransferCaptureResult.Rejected(WorldSectionTransferStatus.InvalidSection);
      }

      if (batch.WorldEpoch != session.Token.WorldEpoch ||
          (capturesInitialPlan && batch.Version != session.Token.PlanVersion))
      {
        return WorldSectionTransferCaptureResult.Rejected(
          capturesInitialPlan
            ? WorldSectionTransferStatus.StalePlan
            : WorldSectionTransferStatus.StaleWorld);
      }

      foreach (WorldSectionSnapshot snapshot in batch.Sections)
      {
        session.Sections[snapshot.Coordinate].Snapshot = snapshot;
        session.Sections[snapshot.Coordinate].State = WorldSectionTransferState.Captured;
      }
    }

    WorldSectionSnapshot[] snapshots = requested
      .Select(coordinate => session.Sections[coordinate].Snapshot)
      .Where(snapshot => snapshot is not null)
      .Cast<WorldSectionSnapshot>()
      .ToArray();
    return WorldSectionTransferCaptureResult.Success(
      needsCapture.Length > 0
        ? WorldSectionTransferStatus.Captured
        : WorldSectionTransferStatus.Accepted,
      snapshots);
  }

  private bool TryGetActiveSession(
    WorldSectionTransferToken token,
    out TransferSession? session,
    out WorldHeaderCapture? current,
    out WorldSectionTransferStatus status)
  {
    session = null;
    current = null;
    status = WorldSectionTransferStatus.Accepted;
    if (!token.IsValid)
    {
      status = WorldSectionTransferStatus.InvalidInput;
      return false;
    }

    if (!_sessions.TryGetValue(token.TransferId, out TransferSession? found) ||
        found.Token != token)
    {
      status = WorldSectionTransferStatus.InvalidSession;
      return false;
    }

    if (found.Invalidated)
    {
      status = WorldSectionTransferStatus.SessionInvalidated;
      return false;
    }

    if (!TryCaptureCurrentHeader(out current) ||
        current is null ||
        current.WorldEpoch != token.WorldEpoch)
    {
      found.Invalidated = true;
      status = WorldSectionTransferStatus.StaleWorld;
      return false;
    }

    session = found;
    return true;
  }

  private bool TryCaptureCurrentHeader(out WorldHeaderCapture? header)
  {
    try
    {
      header = _world.CaptureWorldHeader();
      return true;
    }
    catch (InvalidOperationException)
    {
      header = null;
      return false;
    }
  }

  private static bool TryNormalizeSections(
    IReadOnlyCollection<WorldSectionCoordinate> sections,
    WorldGeometry geometry,
    int maximumCount,
    out WorldSectionCoordinate[] normalized,
    out WorldSectionTransferStatus status)
  {
    normalized = Array.Empty<WorldSectionCoordinate>();
    status = WorldSectionTransferStatus.Accepted;
    if (sections.Count == 0 || sections.Count > maximumCount)
    {
      status = WorldSectionTransferStatus.BudgetExceeded;
      return false;
    }

    var unique = new HashSet<WorldSectionCoordinate>();
    foreach (WorldSectionCoordinate section in sections)
    {
      if (!unique.Add(section))
      {
        status = WorldSectionTransferStatus.DuplicateRequest;
        return false;
      }

      if (!geometry.ContainsSection(section))
      {
        status = WorldSectionTransferStatus.InvalidSection;
        return false;
      }
    }

    normalized = unique
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray();
    return true;
  }

  private sealed class TransferSession(
    WorldSectionTransferToken token,
    WorldGeometry geometry,
    IReadOnlyCollection<WorldSectionCoordinate> initialSections)
  {
    public WorldSectionTransferToken Token { get; } = token;

    public WorldGeometry Geometry { get; } = geometry;

    public IReadOnlyList<WorldSectionCoordinate> InitialSections { get; } =
      Array.AsReadOnly(initialSections
        .OrderBy(section => section.Y)
        .ThenBy(section => section.X)
        .ToArray());

    public HashSet<WorldSectionCoordinate> InitialSectionSet { get; } =
      initialSections.ToHashSet();

    public Dictionary<WorldSectionCoordinate, SectionTransferEntry> Sections { get; set; } =
      initialSections.ToDictionary(
        section => section,
        _ => new SectionTransferEntry());

    public bool Invalidated { get; set; }
  }

  private sealed class SectionTransferEntry
  {
    public WorldSectionTransferState State { get; set; } = WorldSectionTransferState.NotScheduled;

    public WorldSectionSnapshot? Snapshot { get; set; }

    public CommitVersion? PendingDeltaVersion { get; set; }

    public long QueueOrder { get; set; }
  }
}
