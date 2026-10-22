using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>
/// Owns the published tile map and world header. Call mutation and publication methods from the
/// single server commit loop; the internal lock also prevents a torn read if a caller violates it.
/// Chest, sign, TileEntity, player, NPC and event state remain owned by their respective domains.
/// </summary>
public sealed partial class WorldOwner
{
  private readonly object _gate = new();
  private readonly Guid _ownerToken = Guid.NewGuid();
  private readonly WorldFacts _facts;
  private readonly ILiquidRandomSource? _liquidRandomSource;
  private LiquidSimulation _liquidSimulation = new();
  private WorldHeaderSnapshot? _header;
  private WorldTile[]? _tiles;
  private Dictionary<WorldSectionCoordinate, CommitVersion> _dirtySections = [];
  private Dictionary<TileCoordinate, CommitVersion> _dirtyLiquidTiles = [];
  private ulong _worldEpoch;
  private long _commitVersion;

  public WorldOwner(WorldFacts facts, ILiquidRandomSource? liquidRandomSource = null)
  {
    ArgumentNullException.ThrowIfNull(facts);
    _facts = facts;
    _liquidRandomSource = liquidRandomSource;
  }

  public bool IsReady
  {
    get
    {
      lock (_gate)
      {
        return _header is not null && _tiles is not null;
      }
    }
  }

  public WorldFacts Facts => _facts;

  public bool IsCurrentWorldEpoch(ulong worldEpoch)
  {
    lock (_gate)
    {
      return IsCurrentWorld(worldEpoch);
    }
  }

  public WorldPreparedWorld PrepareWorld(WorldPreparationRequest request)
  {
    ArgumentNullException.ThrowIfNull(request);
    lock (_gate)
    {
      return new WorldPreparedWorld(
        request.Header,
        request.Tiles,
        request.FactsFingerprint,
        _ownerToken,
        _worldEpoch,
        CurrentVersion);
    }
  }

  public WorldValidationResult ValidatePreparedWorld(WorldPreparedWorld prepared)
  {
    ArgumentNullException.ThrowIfNull(prepared);
    lock (_gate)
    {
      return ValidatePreparedWorldCore(prepared);
    }
  }

  public WorldPublicationResult CommitPreparedWorld(WorldPreparedWorld prepared)
  {
    ArgumentNullException.ThrowIfNull(prepared);
    lock (_gate)
    {
      WorldValidationResult validation = ValidatePreparedWorldCore(prepared);
      if (!validation.IsValid)
      {
        return WorldPublicationResult.Rejected(_worldEpoch, string.Join("; ", validation.Errors));
      }

      if (_worldEpoch == ulong.MaxValue)
      {
        return WorldPublicationResult.Rejected(_worldEpoch, "The world epoch is exhausted.");
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldPublicationResult.Rejected(_worldEpoch, "The world commit version is exhausted.");
      }

      long nextCommitVersion = _commitVersion == 0 ? CommitVersion.Initial.Value : _commitVersion + 1;
      ulong nextWorldEpoch = _worldEpoch + 1;
      var nextVersion = new CommitVersion(nextCommitVersion);
      WorldTile[] candidateTiles = prepared.Tiles.ToArray();
      Dictionary<WorldSectionCoordinate, CommitVersion> candidateDirtySections =
        AllSections(prepared.Header.Geometry)
          .ToDictionary(section => section, _ => nextVersion);
      var candidateLiquidSimulation = new LiquidSimulation(
        LiquidSimulationOptions.ForGeometry(prepared.Header.Geometry));
      if (!candidateLiquidSimulation.EnqueueUnsettledTiles(
          candidateTiles, prepared.Header.Geometry, _facts)) {
        return WorldPublicationResult.Rejected(
            _worldEpoch, "The loaded world exceeds the bounded unsettled-liquid queue capacity.");
      }

      WorldPublicationResult result = WorldPublicationResult.Success(nextWorldEpoch, nextVersion);
      _header = prepared.Header;
      _tiles = candidateTiles;
      _worldEpoch = nextWorldEpoch;
      _commitVersion = nextCommitVersion;
      _dirtySections = candidateDirtySections;
      _dirtyLiquidTiles = [];
      _liquidSimulation = candidateLiquidSimulation;
      return result;
    }
  }

  public WorldSnapshot CaptureSnapshot()
  {
    lock (_gate)
    {
      EnsureReady();
      return new WorldSnapshot(
        _header!,
        _facts,
        _worldEpoch,
        CurrentVersion,
        _tiles!,
        _dirtySections.Keys.ToArray());
    }
  }

  /// <summary>Copies only the requested region under the same lock as header/epoch/version.</summary>
  public WorldSpatialSnapshot CaptureSpatialSnapshot(WorldSectionBounds bounds) {
    lock (_gate) {
      EnsureReady();
      WorldGeometry geometry = _header!.Geometry;
      if (bounds.Width <= 0 || bounds.Height <= 0 || bounds.OriginX < 0 || bounds.OriginY < 0 ||
          (long)bounds.OriginX + bounds.Width > geometry.Width ||
          (long)bounds.OriginY + bounds.Height > geometry.Height ||
          (long)bounds.Width * bounds.Height > WorldSpatialSnapshot.MaximumCaptureTiles) {
        throw new ArgumentOutOfRangeException(nameof(bounds));
      }
      var tiles = new WorldTile[bounds.Width * bounds.Height];
      for (int y = 0; y < bounds.Height; y++) {
        Array.Copy(_tiles!, (bounds.OriginY + y) * geometry.Width + bounds.OriginX,
            tiles, y * bounds.Width, bounds.Width);
      }
      return new WorldSpatialSnapshot(_header!, _facts, _worldEpoch, CurrentVersion, bounds, tiles);
    }
  }

  public bool IsCurrentWorldRevision(ulong worldEpoch, CommitVersion version) {
    lock (_gate) {
      return IsCurrentWorld(worldEpoch) && version == CurrentVersion;
    }
  }

  /// <summary>Captures each anchor's own scene while holding one common world revision.</summary>
  public IReadOnlyList<WorldSceneMetrics> CapturePylonScenes(
      IReadOnlyList<TileCoordinate> anchors, bool infectedSeed) {
    ArgumentNullException.ThrowIfNull(anchors);
    if (anchors.Count > 256) {
      throw new ArgumentOutOfRangeException(nameof(anchors));
    }
    lock (_gate) {
      EnsureReady();
      var scenes = new List<WorldSceneMetrics>(anchors.Count);
      foreach (TileCoordinate anchor in anchors) {
        WorldSectionBounds bounds = WorldSceneMetrics.GetScanBounds(_header!.Geometry, anchor);
        WorldSpatialSnapshot snapshot = CaptureSpatialSnapshot(bounds);
        scenes.Add(WorldSceneMetrics.Capture(snapshot, anchor, infectedSeed));
      }
      return scenes.AsReadOnly();
    }
  }

  /// <summary>Captures packet-7 world inputs without copying the authoritative tile map.</summary>
  public WorldHeaderCapture CaptureWorldHeader()
  {
    lock (_gate)
    {
      EnsureReady();
      return new WorldHeaderCapture(_header!, _facts, _worldEpoch, CurrentVersion);
    }
  }

  /// <summary>
  /// Captures the World-owned inputs used to prepare one authoritative server tick. The capture
  /// must still match this owner when the tick is committed.
  /// </summary>
  public WorldTickCapture CaptureWorldTick()
  {
    lock (_gate)
    {
      EnsureReady();
      return new WorldTickCapture(
        _ownerToken,
        _header!,
        _facts,
        _worldEpoch,
        CurrentVersion);
    }
  }

  /// <summary>
  /// Plans the initial section neighborhood for the published world. Player identity and phase
  /// remain outside this owner; only supplied tile positions and portal sections are considered.
  /// </summary>
  public WorldSectionPlan PlanInitialSections(
    TileCoordinate? playerSpawn,
    IReadOnlyCollection<TileCoordinate> additionalSpawns,
    IReadOnlyCollection<WorldSectionCoordinate> portalSections)
  {
    ArgumentNullException.ThrowIfNull(additionalSpawns);
    ArgumentNullException.ThrowIfNull(portalSections);
    lock (_gate)
    {
      EnsureReady();
      WorldGeometry geometry = _header!.Geometry;
      var sections = new HashSet<WorldSectionCoordinate>();
      AddSectionNeighborhood(sections, geometry, _header.Spawn);
      if (playerSpawn.HasValue && geometry.Contains(playerSpawn.Value))
      {
        AddSectionNeighborhood(sections, geometry, playerSpawn.Value);
      }

      foreach (TileCoordinate spawn in additionalSpawns)
      {
        if (geometry.Contains(spawn))
        {
          AddSectionNeighborhood(sections, geometry, spawn);
        }
      }

      foreach (WorldSectionCoordinate portalSection in portalSections)
      {
        if (!geometry.ContainsSection(portalSection))
        {
          throw new ArgumentOutOfRangeException(nameof(portalSections));
        }

        sections.Add(portalSection);
      }

      return new WorldSectionPlan(_worldEpoch, CurrentVersion, sections);
    }
  }

  /// <summary>
  /// Captures exactly the sections from a current plan; stale plans cannot mix revisions.
  /// </summary>
  public WorldSectionSnapshotBatch CaptureSections(WorldSectionPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    lock (_gate)
    {
      EnsureReady();
      if (plan.WorldEpoch != _worldEpoch || plan.Version != CurrentVersion)
      {
        throw new InvalidOperationException(
          "The section plan no longer describes the current published world revision.");
      }

      return new WorldSectionSnapshotBatch(
        _worldEpoch,
        CurrentVersion,
        plan.Sections.Select(CaptureSectionCore).ToArray());
    }
  }

  /// <summary>Captures a defensive snapshot for the P08 persistence boundary.</summary>
  public WorldSaveSnapshot CaptureSaveSnapshot()
  {
    return new WorldSaveSnapshot(CaptureSnapshot());
  }

  /// <summary>Commits one deterministic world-time advance on the server commit loop.</summary>
  public WorldMutationResult AdvanceWorldTime(
    ulong worldEpoch,
    double elapsedGameTicks,
    int dayRate)
  {
    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return WorldMutationResult.Rejected("The world epoch is stale or unavailable.");
      }

      WorldTimeState current = new(_header!.DayTime, _header.Time, _header.MoonPhase);
      WorldTimeState next;
      try
      {
        next = WorldTimeSystem.Advance(current, elapsedGameTicks, dayRate);
      }
      catch (ArgumentOutOfRangeException)
      {
        return WorldMutationResult.Rejected("The world time advance input is invalid or overflowed.");
      }

      if (next == current)
      {
        return WorldMutationResult.Success(
          CurrentVersion,
          Array.Empty<TileCoordinate>(),
          Array.Empty<WorldSectionCoordinate>(),
          producesImmediateBroadcast: false);
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldMutationResult.Rejected("The world commit version is exhausted.");
      }

      var nextHeader = new WorldHeaderSnapshot(
        _header.Name,
        _header.Geometry,
        _header.Spawn,
        _header.UniqueId,
        next.DayTime,
        next.Time,
        next.MoonPhase,
        _header.WorldId,
        _header.Facts,
        _header.TeamSpawns);
      CommitVersion version = NextVersion();
      WorldMutationResult result = WorldMutationResult.Success(
        version,
        Array.Empty<TileCoordinate>(),
        Array.Empty<WorldSectionCoordinate>(),
        producesImmediateBroadcast: false);

      _header = nextHeader;
      _commitVersion = version.Value;
      return result;
    }
  }

  /// <summary>
  /// Stages one server tick's time and liquid work and publishes them as one commit. A liquid
  /// queue rejection therefore cannot leave a successful time-only commit behind it.
  /// </summary>
  internal WorldTickMutationResult AdvanceWorldTick(
    WorldTickCapture capture,
    double elapsedGameTicks,
    int dayRate,
    int operationBudget)
  {
    ArgumentNullException.ThrowIfNull(capture);
    lock (_gate)
    {
      if (capture.OwnerToken != _ownerToken)
      {
        return WorldTickMutationResult.Rejected(
          CurrentVersion,
          "The world tick capture belongs to another owner.");
      }

      if (!IsCurrentWorld(capture.WorldEpoch))
      {
        return WorldTickMutationResult.Rejected(
          CurrentVersion,
          "The world epoch is stale or unavailable.");
      }

      if (capture.Version != CurrentVersion)
      {
        return WorldTickMutationResult.Rejected(
          CurrentVersion,
          "The world tick capture version is stale.");
      }

      if (operationBudget <= 0)
      {
        return WorldTickMutationResult.Rejected(
          CurrentVersion,
          "The liquid operation budget must be positive.");
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldTickMutationResult.Rejected(
          CurrentVersion,
          "The world commit version is exhausted.");
      }

      WorldTimeState currentTime = new(_header!.DayTime, _header.Time, _header.MoonPhase);
      if (capture.Time != currentTime)
      {
        return WorldTickMutationResult.Rejected(
          CurrentVersion,
          "The world tick capture time does not match the published revision.");
      }

      WorldTimeAdvanceResult timeAdvance;
      try
      {
        timeAdvance = WorldTimeSystem.CalculateAdvance(currentTime, elapsedGameTicks, dayRate);
      }
      catch (ArgumentOutOfRangeException)
      {
        return WorldTickMutationResult.Rejected(
          CurrentVersion,
          "The world time advance input is invalid or overflowed.");
      }

      WorldTimeState nextTime = timeAdvance.State;

      if (RequiresLiquidRandomSource()) {
        return WorldTickMutationResult.Rejected(
            CurrentVersion, "The host liquid random source is unavailable.");
      }
      LiquidSimulation nextLiquid = _liquidSimulation.Clone();
      LiquidStepResult step = nextLiquid.Step(
        _tiles!,
        _header.Geometry,
        _facts,
        operationBudget,
        _liquidRandomSource);
      if (step.QueueCapacityExceeded)
      {
        return WorldTickMutationResult.Rejected(
          CurrentVersion,
          "The bounded liquid active/buffer queues are full; the tick was not committed.");
      }

      bool timeChanged = nextTime != currentTime || timeAdvance.MorningTransitionCount > 0;
      if (!timeChanged && step.ChangedTiles.Count == 0)
      {
        _liquidSimulation = nextLiquid;
        return WorldTickMutationResult.Success(
          CurrentVersion,
          timeChanged: false,
          morningTransitionCount: 0,
          Array.Empty<TileCoordinate>(),
          _dirtySections.Keys.ToArray(),
          step.MergeActions);
      }

      CommitVersion version = NextVersion();
      var dirtySections = new HashSet<WorldSectionCoordinate>();
      foreach (TileCoordinate coordinate in step.ChangedTiles)
      {
        // Liquid changes can alter the visible frame at a section edge. Keep the same
        // one-tile framing neighborhood used by ordinary tile mutations.
        AddFramingSections(dirtySections, coordinate, _header!.Geometry);
      }

      if (timeChanged)
      {
        _header = new WorldHeaderSnapshot(
          _header.Name,
          _header.Geometry,
          _header.Spawn,
          _header.UniqueId,
          nextTime.DayTime,
          nextTime.Time,
          nextTime.MoonPhase,
          _header.WorldId,
          _header.Facts,
          _header.TeamSpawns);
      }

      foreach (KeyValuePair<int, WorldTile> update in step.TileUpdates)
      {
        _tiles![update.Key] = update.Value;
        _dirtyLiquidTiles[GetCoordinate(update.Key)] = version;
      }

      _liquidSimulation = nextLiquid;
      _commitVersion = version.Value;
      _dirtySections = WithDirtySections(_dirtySections, dirtySections, version);
      return WorldTickMutationResult.Success(
        version,
        timeChanged,
        timeAdvance.MorningTransitionCount,
        step.ChangedTiles,
        _dirtySections.Keys.ToArray(),
        step.MergeActions);
    }
  }

  public WorldAlignmentMetrics CalculateAlignmentMetrics()
  {
    return CaptureAlignmentSnapshot().Metrics;
  }

  public WorldAlignmentSnapshot CaptureAlignmentSnapshot()
  {
    lock (_gate)
    {
      EnsureReady();
      WorldAlignmentMetrics metrics = WorldAlignmentMetricsCalculator.Calculate(_header!, _tiles!);
      return new WorldAlignmentSnapshot(_worldEpoch, CurrentVersion, metrics);
    }
  }

  public WorldSectionSnapshot CaptureSection(WorldSectionCoordinate coordinate)
  {
    lock (_gate)
    {
      EnsureReady();
      return CaptureSectionCore(coordinate);
    }
  }

  /// <summary>
  /// Captures several sections atomically. Every section is from the same published epoch and
  /// commit version, so a baseline cannot combine tiles from two world revisions.
  /// </summary>
  public WorldSectionSnapshotBatch CaptureSections(
    IReadOnlyCollection<WorldSectionCoordinate> coordinates)
  {
    ArgumentNullException.ThrowIfNull(coordinates);
    lock (_gate)
    {
      EnsureReady();
      var uniqueCoordinates = new HashSet<WorldSectionCoordinate>();
      foreach (WorldSectionCoordinate coordinate in coordinates)
      {
        if (!uniqueCoordinates.Add(coordinate))
        {
          throw new ArgumentException(
            "A section batch cannot contain duplicate coordinates.",
            nameof(coordinates));
        }
      }

      WorldSectionSnapshot[] sections = uniqueCoordinates
        .OrderBy(coordinate => coordinate.Y)
        .ThenBy(coordinate => coordinate.X)
        .Select(CaptureSectionCore)
        .ToArray();
      return new WorldSectionSnapshotBatch(_worldEpoch, CurrentVersion, sections);
    }
  }

  public bool IsInsideUnbreakableWalls(TileCoordinate coordinate)
  {
    lock (_gate)
    {
      EnsureReady();
      return UnbreakableWallScanner.IsInside(coordinate, _header!.Geometry, _tiles!);
    }
  }

  public WorldPublicationResult UnloadWorld()
  {
    lock (_gate)
    {
      if (_worldEpoch == ulong.MaxValue)
      {
        return WorldPublicationResult.Rejected(_worldEpoch, "The world epoch is exhausted.");
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldPublicationResult.Rejected(_worldEpoch, "The world commit version is exhausted.");
      }

      _header = null;
      _tiles = null;
      _worldEpoch++;
      _commitVersion++;
      _dirtySections.Clear();
      _dirtyLiquidTiles.Clear();
      _liquidSimulation.Reset();
      return WorldPublicationResult.Success(_worldEpoch, CurrentVersion);
    }
  }

  public WorldMutationResult ApplyTileDeltas(
    ulong worldEpoch,
    IReadOnlyList<KeyValuePair<TileCoordinate, WorldTileDelta>> deltas,
    bool producesImmediateBroadcast)
  {
    ArgumentNullException.ThrowIfNull(deltas);
    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return WorldMutationResult.Rejected("The world epoch is stale or unavailable.");
      }

      if (deltas.Count == 0)
      {
        return WorldMutationResult.Success(
          CurrentVersion,
          Array.Empty<TileCoordinate>(),
          Array.Empty<WorldSectionCoordinate>(),
          producesImmediateBroadcast);
      }

      var candidate = new Dictionary<int, WorldTile>();
      foreach (KeyValuePair<TileCoordinate, WorldTileDelta> change in deltas)
      {
        if (!_header!.Geometry.Contains(change.Key))
        {
          return WorldMutationResult.Rejected($"Tile coordinate {change.Key} is outside the world.");
        }

        int index = GetTileIndex(change.Key);
        WorldTile current = candidate.TryGetValue(index, out WorldTile staged)
          ? staged
          : _tiles![index];
        if (!TryStageTileDelta(current, change.Value, out WorldTile next))
        {
          return WorldMutationResult.Rejected($"Tile change at {change.Key} contains invalid facts.");
        }

        candidate[index] = next;
      }

      var changedTiles = new List<TileCoordinate>();
      var changedSections = new HashSet<WorldSectionCoordinate>();
      foreach (KeyValuePair<int, WorldTile> change in candidate)
      {
        if (_tiles![change.Key] == change.Value)
        {
          continue;
        }

        TileCoordinate coordinate = GetCoordinate(change.Key);
        changedTiles.Add(coordinate);
        AddFramingSections(changedSections, coordinate, _header!.Geometry);
      }

      if (changedTiles.Count == 0)
      {
        return WorldMutationResult.Success(
          CurrentVersion,
          changedTiles,
          changedSections,
          producesImmediateBroadcast);
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldMutationResult.Rejected("The world commit version is exhausted.");
      }

      CommitVersion version = NextVersion();
      Dictionary<WorldSectionCoordinate, CommitVersion> nextDirtySections =
        WithDirtySections(_dirtySections, changedSections, version);
      WorldMutationResult result = WorldMutationResult.Success(
        version,
        changedTiles,
        changedSections,
        producesImmediateBroadcast);

      LiquidSimulation nextLiquid = _liquidSimulation.Clone();
      foreach (TileCoordinate coordinate in changedTiles)
      {
        if (!nextLiquid.EnqueueAround(coordinate, _header!.Geometry))
        {
          return WorldMutationResult.Rejected(
              "The bounded liquid queue could not accept the tile footprint.");
        }
      }

      foreach (KeyValuePair<int, WorldTile> change in candidate)
      {
        if (_tiles![change.Key] != change.Value)
        {
          WorldTile previous = _tiles[change.Key];
          _tiles[change.Key] = change.Value;
          if (previous.LiquidAmount != change.Value.LiquidAmount ||
              previous.LiquidType != change.Value.LiquidType)
          {
            _dirtyLiquidTiles[GetCoordinate(change.Key)] = version;
          }
        }
      }

      nextLiquid.InvalidatePanicContinuation();
      _liquidSimulation = nextLiquid;

      _commitVersion = version.Value;
      _dirtySections = nextDirtySections;
      return result;
    }
  }

  /// <summary>
  /// Applies a validated Packet 20 patch while preserving every field omitted on the wire.
  /// </summary>
  public WorldMutationResult ApplyTilePatches(
    ulong worldEpoch,
    IReadOnlyList<WorldTilePatch> patches,
    bool producesImmediateBroadcast)
  {
    ArgumentNullException.ThrowIfNull(patches);
    var deltas = new List<KeyValuePair<TileCoordinate, WorldTileDelta>>(patches.Count);
    foreach (WorldTilePatch patch in patches)
    {
      deltas.Add(new KeyValuePair<TileCoordinate, WorldTileDelta>(
        patch.Coordinate,
        patch.ToDelta()));
    }

    return ApplyTileDeltas(worldEpoch, deltas, producesImmediateBroadcast);
  }

  /// <summary>
  /// Commits module 0's direct liquid assignments. The Version4 reader does not frame, schedule,
  /// dirty-network or broadcast as an immediate consequence of this operation.
  /// </summary>
  public WorldMutationResult ApplyLiquidModuleValues(
    ulong worldEpoch,
    IReadOnlyList<LiquidUpdate> updates,
    int maximumUpdates)
  {
    ArgumentNullException.ThrowIfNull(updates);
    if (maximumUpdates <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumUpdates));
    }

    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return WorldMutationResult.Rejected("The world epoch is stale or unavailable.");
      }

      if (updates.Count > maximumUpdates)
      {
        return WorldMutationResult.Rejected("The liquid update count exceeds the bounded commit budget.");
      }

      var staged = new Dictionary<int, WorldTile>();
      foreach (LiquidUpdate update in updates)
      {
        if (!_header!.Geometry.Contains(update.Coordinate) || !Enum.IsDefined(update.Type))
        {
          return WorldMutationResult.Rejected(
            $"Liquid update at {update.Coordinate} has an invalid coordinate or type.");
        }

        int index = GetTileIndex(update.Coordinate);
        WorldTile current = staged.TryGetValue(index, out WorldTile value) ? value : _tiles![index];
        staged[index] = current.WithLiquid(update.Amount, update.Type);
      }

      var changedTiles = new List<TileCoordinate>();
      var changedSections = new HashSet<WorldSectionCoordinate>();
      foreach (KeyValuePair<int, WorldTile> update in staged)
      {
        if (_tiles![update.Key] == update.Value)
        {
          continue;
        }

        TileCoordinate coordinate = GetCoordinate(update.Key);
        changedTiles.Add(coordinate);
      }

      if (changedTiles.Count == 0)
      {
        return WorldMutationResult.Success(
          CurrentVersion,
          changedTiles,
          changedSections,
          producesImmediateBroadcast: false);
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldMutationResult.Rejected("The world commit version is exhausted.");
      }

      CommitVersion version = NextVersion();
      WorldMutationResult result = WorldMutationResult.Success(
        version,
        changedTiles,
        changedSections,
        producesImmediateBroadcast: false);

      foreach (KeyValuePair<int, WorldTile> update in staged)
      {
        if (_tiles![update.Key] != update.Value)
        {
          _tiles[update.Key] = update.Value;
        }
      }

      _liquidSimulation.InvalidatePanicContinuation();

      _commitVersion = version.Value;
      return result;
    }
  }

  private bool RequiresLiquidRandomSource() {
    return _liquidRandomSource is null &&
        (_liquidSimulation.PendingCount > 0 || _liquidSimulation.Status.PanicMode);
  }

  public WorldMutationResult AdvanceLiquidSimulation(
    ulong worldEpoch,
    int operationBudget)
  {
    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return WorldMutationResult.Rejected("The world epoch is stale or unavailable.");
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldMutationResult.Rejected("The world commit version is exhausted.");
      }

      if (RequiresLiquidRandomSource()) {
        return WorldMutationResult.Rejected("The host liquid random source is unavailable.");
      }
      LiquidSimulation nextLiquid = _liquidSimulation.Clone();
      LiquidStepResult step = nextLiquid.Step(
        _tiles!,
        _header!.Geometry,
        _facts,
        operationBudget,
        _liquidRandomSource);
      if (step.QueueCapacityExceeded)
      {
        return WorldMutationResult.Rejected(
          "The bounded liquid active/buffer queues are full; the step was not committed.");
      }

      if (step.ChangedTiles.Count == 0)
      {
        _liquidSimulation = nextLiquid;
        return WorldMutationResult.Success(
          CurrentVersion,
          Array.Empty<TileCoordinate>(),
          Array.Empty<WorldSectionCoordinate>(),
          producesImmediateBroadcast: false,
          mergeActions: step.MergeActions);
      }

      var uniqueChanges = step.ChangedTiles.ToArray();
      var dirtySections = new HashSet<WorldSectionCoordinate>();
      foreach (TileCoordinate coordinate in uniqueChanges)
      {
        AddFramingSections(dirtySections, coordinate, _header!.Geometry);
      }

      CommitVersion version = NextVersion();
      Dictionary<WorldSectionCoordinate, CommitVersion> nextDirtySections =
        WithDirtySections(_dirtySections, dirtySections, version);
      WorldMutationResult result = WorldMutationResult.Success(
        version,
        uniqueChanges,
        dirtySections,
        producesImmediateBroadcast: false,
        mergeActions: step.MergeActions);

      foreach (KeyValuePair<int, WorldTile> update in step.TileUpdates)
      {
        _tiles![update.Key] = update.Value;
        _dirtyLiquidTiles[GetCoordinate(update.Key)] = version;
      }

      _liquidSimulation = nextLiquid;
      _commitVersion = version.Value;
      _dirtySections = nextDirtySections;
      return result;
    }
  }

  /// <summary>
  /// Runs bounded loading/recovery liquid settling and publishes its overlay in one commit.
  /// Partial progress remains queued for a later call; no network effect is produced here.
  /// </summary>
  public WorldLiquidSettleResult SettleLiquidSimulation(
    ulong worldEpoch,
    int maximumOperations)
  {
    if (maximumOperations <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumOperations));
    }

    lock (_gate)
    {
      if (!IsCurrentWorld(worldEpoch))
      {
        return WorldLiquidSettleResult.Rejected("The world epoch is stale or unavailable.");
      }

      if (_commitVersion == long.MaxValue)
      {
        return WorldLiquidSettleResult.Rejected("The world commit version is exhausted.");
      }

      if (RequiresLiquidRandomSource()) {
        return WorldLiquidSettleResult.Rejected("The host liquid random source is unavailable.");
      }
      LiquidSimulation nextLiquid = _liquidSimulation.Clone();
      LiquidQuickSettleResult settle = nextLiquid.QuickSettle(
        _tiles!,
        _header!.Geometry,
        _facts,
        maximumOperations,
        _liquidRandomSource);
      if (settle.QueueCapacityExceeded)
      {
        return WorldLiquidSettleResult.Rejected(
          "The bounded liquid active/buffer queues are full; settling was not committed.");
      }

      var updates = new Dictionary<int, WorldTile>();
      var changedTiles = new List<TileCoordinate>();
      var changedSections = new HashSet<WorldSectionCoordinate>();
      foreach (KeyValuePair<int, WorldTile> update in settle.TileUpdates)
      {
        if (_tiles![update.Key] == update.Value)
        {
          continue;
        }

        updates[update.Key] = update.Value;
        TileCoordinate coordinate = GetCoordinate(update.Key);
        changedTiles.Add(coordinate);
        AddFramingSections(changedSections, coordinate, _header!.Geometry);
      }

      if (updates.Count == 0)
      {
        _liquidSimulation = nextLiquid;
        return WorldLiquidSettleResult.Success(
          nextLiquid.PendingCount == 0 && !nextLiquid.Status.PanicMode,
          settle.OperationsUsed,
          null,
          changedTiles,
          changedSections,
          settle.MergeActions);
      }

      CommitVersion version = NextVersion();
      Dictionary<WorldSectionCoordinate, CommitVersion> nextDirtySections =
        WithDirtySections(_dirtySections, changedSections, version);
      foreach (KeyValuePair<int, WorldTile> update in updates)
      {
        if (!nextLiquid.EnqueueAround(GetCoordinate(update.Key), _header.Geometry))
        {
          return WorldLiquidSettleResult.Rejected(
            "The bounded liquid active/buffer queues are full; settling was not committed.");
        }
      }

      bool completed = nextLiquid.PendingCount == 0 && !nextLiquid.Status.PanicMode;
      var nextDirtyLiquids = new Dictionary<TileCoordinate, CommitVersion>(_dirtyLiquidTiles);
      foreach (KeyValuePair<int, WorldTile> update in updates)
      {
        WorldTile previous = _tiles![update.Key];
        if (previous.LiquidAmount != update.Value.LiquidAmount ||
            previous.LiquidType != update.Value.LiquidType)
        {
          nextDirtyLiquids[GetCoordinate(update.Key)] = version;
        }
      }

      foreach (KeyValuePair<int, WorldTile> update in updates)
      {
        _tiles![update.Key] = update.Value;
      }
      _liquidSimulation = nextLiquid;
      _dirtyLiquidTiles = nextDirtyLiquids;
      _commitVersion = version.Value;
      _dirtySections = nextDirtySections;
      return WorldLiquidSettleResult.Success(
        completed,
        settle.OperationsUsed,
        version,
        changedTiles,
        changedSections,
        settle.MergeActions);
    }
  }

  public WorldDirtyBatch CaptureDirtySections()
  {
    lock (_gate)
    {
      EnsureReady();
      var captured = new Dictionary<WorldSectionCoordinate, CommitVersion>(_dirtySections);
      return new WorldDirtyBatch(_ownerToken, _worldEpoch, CurrentVersion, captured);
    }
  }

  /// <summary>Captures latest changed liquid values for a bounded network delta.</summary>
  public WorldLiquidDirtyBatch CaptureDirtyLiquidValues(int maximumEntries)
  {
    if (maximumEntries <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumEntries));
    }

    lock (_gate)
    {
      EnsureReady();
      var values = new List<LiquidDirtyValue>(Math.Min(maximumEntries, _dirtyLiquidTiles.Count));
      foreach (KeyValuePair<TileCoordinate, CommitVersion> pair in _dirtyLiquidTiles
        .OrderBy(pair => pair.Value.Value)
        .ThenBy(pair => pair.Key.Y)
        .ThenBy(pair => pair.Key.X)
        .Take(maximumEntries))
      {
        WorldTile tile = _tiles![(pair.Key.Y * _header!.Geometry.Width) + pair.Key.X];
        values.Add(new LiquidDirtyValue(pair.Key, tile.LiquidAmount, tile.LiquidType, pair.Value));
      }

      return new WorldLiquidDirtyBatch(_ownerToken, _worldEpoch, CurrentVersion, values);
    }
  }

  /// <summary>Clears liquid entries only if no newer update changed their captured version.</summary>
  public bool AcknowledgeDirtyLiquidValues(WorldLiquidDirtyBatch batch)
  {
    ArgumentNullException.ThrowIfNull(batch);
    lock (_gate)
    {
      if (batch.OwnerToken != _ownerToken || batch.WorldEpoch != _worldEpoch)
      {
        return false;
      }

      foreach (LiquidDirtyValue value in batch.Values)
      {
        if (_dirtyLiquidTiles.TryGetValue(value.Coordinate, out CommitVersion current) &&
            current == value.Version)
        {
          _dirtyLiquidTiles.Remove(value.Coordinate);
        }
      }

      return true;
    }
  }

  /// <summary>
  /// Clears only sections whose captured version was actually queued for sending. A newer update
  /// to the same section remains dirty for the next batch.
  /// </summary>
  public bool AcknowledgeDirtySections(
    WorldDirtyBatch batch,
    IReadOnlyCollection<WorldSectionCoordinate> queuedSections)
  {
    ArgumentNullException.ThrowIfNull(batch);
    ArgumentNullException.ThrowIfNull(queuedSections);
    lock (_gate)
    {
      if (batch.OwnerToken != _ownerToken || batch.WorldEpoch != _worldEpoch)
      {
        return false;
      }

      foreach (WorldSectionCoordinate section in queuedSections)
      {
        if (batch.SectionVersions.TryGetValue(section, out CommitVersion capturedVersion) &&
            _dirtySections.TryGetValue(section, out CommitVersion currentVersion) &&
            currentVersion == capturedVersion)
        {
          _dirtySections.Remove(section);
        }
      }

      return true;
    }
  }

  private WorldValidationResult ValidatePreparedWorldCore(WorldPreparedWorld prepared)
  {
    var errors = new List<string>();
    if (prepared.OwnerToken != _ownerToken)
    {
      errors.Add("The prepared world belongs to a different world owner.");
    }

    if (prepared.BaseWorldEpoch != _worldEpoch)
    {
      errors.Add("The active world changed after this candidate was prepared.");
    }

    if (prepared.BaseCommitVersion != CurrentVersion)
    {
      errors.Add("The world changed after this candidate was prepared.");
    }

    if (!string.Equals(prepared.FactsFingerprint, _facts.Fingerprint, StringComparison.Ordinal))
    {
      errors.Add("The candidate was decoded using different Version4 world facts.");
    }

    WorldGeometry geometry = prepared.Header.Geometry;
    long tileCount = (long)geometry.Width * geometry.Height;
    if (geometry.Width <= 0 || geometry.Height <= 0 ||
        tileCount > WorldGeometry.MaximumTileCount ||
        tileCount != prepared.Tiles.Count)
    {
      errors.Add("The world dimensions or tile payload exceed the owner limits.");
    }

    if (!geometry.Contains(prepared.Header.Spawn))
    {
      errors.Add("The world spawn is outside the candidate world.");
    }

    if (!double.IsFinite(prepared.Header.Time) || prepared.Header.Time < 0 ||
        prepared.Header.Time > (prepared.Header.DayTime
          ? WorldTimeState.DayLength : WorldTimeState.NightLength))
    {
      errors.Add("The world time is outside the Version4 day or night range.");
    }

    if (prepared.Header.MoonPhase > 7)
    {
      errors.Add("The moon phase is outside the Version4 range.");
    }

    for (int index = 0; index < prepared.Tiles.Count; index++)
    {
      WorldTile tile = prepared.Tiles[index];
      if (!_facts.IsValidTileType(tile.Type) || tile.Wall >= WorldFacts.WallCount ||
          !Enum.IsDefined(tile.LiquidType) ||
          (((byte)tile.Wires) & 0xf0) != 0 || tile.Slope > 4 || tile.TileColor > 31 ||
          tile.WallColor > 31 || (tile.TileHeader3 & 0x1f) != 0 || (tile.WallHeader & 0x7f) != 0)
      {
        errors.Add($"Tile index {index} contains an invalid tile or liquid fact.");
        break;
      }

      if (tile.Active && !_facts.FrameImportant[tile.Type] &&
          (tile.FrameX != -1 || tile.FrameY != -1))
      {
        errors.Add($"Non-frame-important tile index {index} carries persisted frame coordinates.");
        break;
      }
    }

    return errors.Count == 0 ? WorldValidationResult.Valid() : WorldValidationResult.Invalid(errors);
  }

  private WorldSectionSnapshot CaptureSectionCore(WorldSectionCoordinate coordinate)
  {
    WorldSectionBounds bounds = _header!.Geometry.GetSectionBounds(coordinate);
    var tiles = new WorldTile[bounds.Width * bounds.Height];
    for (int y = 0; y < bounds.Height; y++)
    {
      Array.Copy(
        _tiles!,
        ((bounds.OriginY + y) * _header.Geometry.Width) + bounds.OriginX,
        tiles,
        y * bounds.Width,
        bounds.Width);
    }

    return new WorldSectionSnapshot(coordinate, bounds, _worldEpoch, CurrentVersion, tiles);
  }

  private bool IsCurrentWorld(ulong worldEpoch)
  {
    return worldEpoch != 0 && worldEpoch == _worldEpoch && _header is not null && _tiles is not null;
  }

  private void EnsureReady()
  {
    if (_header is null || _tiles is null)
    {
      throw new InvalidOperationException("No validated world has been published.");
    }
  }

  private CommitVersion CurrentVersion => new(_commitVersion);

  private CommitVersion NextVersion()
  {
    if (_commitVersion == long.MaxValue)
    {
      throw new InvalidOperationException("The world commit version is exhausted.");
    }

    return new CommitVersion(_commitVersion == 0 ? CommitVersion.Initial.Value : _commitVersion + 1);
  }

  private int GetTileIndex(TileCoordinate coordinate)
  {
    return checked((coordinate.Y * _header!.Geometry.Width) + coordinate.X);
  }

  private TileCoordinate GetCoordinate(int index)
  {
    return new TileCoordinate(index % _header!.Geometry.Width, index / _header.Geometry.Width);
  }

  private WorldSectionCoordinate GetSection(TileCoordinate coordinate)
  {
    return new WorldSectionCoordinate(
      coordinate.X / WorldGeometry.SectionWidth,
      coordinate.Y / WorldGeometry.SectionHeight);
  }

  private static void AddSectionNeighborhood(
    ISet<WorldSectionCoordinate> sections,
    WorldGeometry geometry,
    TileCoordinate spawn)
  {
    foreach (WorldSectionCoordinate section in geometry.GetSectionsAroundPixelPosition(
      spawn.X * 16f,
      spawn.Y * 16f,
      radius: 1))
    {
      sections.Add(section);
    }
  }

  private static HashSet<WorldSectionCoordinate> AllSections(WorldGeometry geometry)
  {
    var sections = new HashSet<WorldSectionCoordinate>();
    for (int y = 0; y < geometry.SectionCountY; y++)
    {
      for (int x = 0; x < geometry.SectionCountX; x++)
      {
        sections.Add(new WorldSectionCoordinate(x, y));
      }
    }

    return sections;
  }

  private static Dictionary<WorldSectionCoordinate, CommitVersion> WithDirtySections(
    IReadOnlyDictionary<WorldSectionCoordinate, CommitVersion> current,
    IEnumerable<WorldSectionCoordinate> sections,
    CommitVersion version)
  {
    var result = current.ToDictionary(pair => pair.Key, pair => pair.Value);
    foreach (WorldSectionCoordinate section in sections)
    {
      result[section] = version;
    }

    return result;
  }

  private static void AddFramingSections(
      ISet<WorldSectionCoordinate> sections,
      TileCoordinate coordinate,
      WorldGeometry geometry)
  {
    int minX = Math.Max(0, coordinate.X - 1);
    int maxX = Math.Min(geometry.Width - 1, coordinate.X + 1);
    int minY = Math.Max(0, coordinate.Y - 1);
    int maxY = Math.Min(geometry.Height - 1, coordinate.Y + 1);
    for (int y = minY; y <= maxY; y++)
    {
      for (int x = minX; x <= maxX; x++)
      {
        sections.Add(new WorldSectionCoordinate(
            x / WorldGeometry.SectionWidth, y / WorldGeometry.SectionHeight));
      }
    }
  }
}

public readonly record struct LiquidDirtyValue(
  TileCoordinate Coordinate,
  byte Amount,
  LiquidType Type,
  CommitVersion Version);

public sealed class WorldLiquidDirtyBatch
{
  internal WorldLiquidDirtyBatch(
    Guid ownerToken,
    ulong worldEpoch,
    CommitVersion version,
    IReadOnlyCollection<LiquidDirtyValue> values)
  {
    OwnerToken = ownerToken;
    WorldEpoch = worldEpoch;
    Version = version;
    Values = Array.AsReadOnly(values.ToArray());
  }

  internal Guid OwnerToken { get; }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public IReadOnlyList<LiquidDirtyValue> Values { get; }
}
