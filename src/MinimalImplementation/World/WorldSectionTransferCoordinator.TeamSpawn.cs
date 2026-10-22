namespace MinimalImplementation.Server.World;

public sealed partial class WorldSectionTransferCoordinator {
  /// <summary>
  /// Resolves the actual persisted team array index and stages only unsent section baselines.
  /// Neither this method nor the returned Validate invalidates or modifies a session.
  /// </summary>
  public bool TryPrepareTeamSpawn(
      WorldSectionTransferToken token, byte team,
      out WorldTeamSpawnPreparation? preparation, out WorldSectionTransferStatus status) {
    lock (_gate) {
      preparation = null;
      if (team > 5) {
        status = WorldSectionTransferStatus.InvalidInput;
        return false;
      }
      if (!TryReadPreparationSession(token, out TransferSession? session,
          out WorldHeaderCapture? header, out status)) {
        return false;
      }

      TransferSession preparedSession = session!;
      WorldHeaderCapture capturedHeader = header!;
      WorldTeamSpawnFacts facts = capturedHeader.Header.TeamSpawns;
      if (!facts.IsReady) {
        status = WorldSectionTransferStatus.FactsUnavailable;
        return false;
      }
      TileCoordinate? spawn = facts.TeamBasedSpawns && team < facts.ExtraSpawnPoints.Count
          ? facts.ExtraSpawnPoints[team]
          : null;
      WorldSectionCoordinate[] required = spawn.HasValue
          ? preparedSession.Geometry.GetSectionsAroundPixelPosition(
              spawn.Value.X * 16f, spawn.Value.Y * 16f, 1).ToArray()
          : [];
      WorldSectionCoordinate[] needsBaseline = required.Where(coordinate =>
          !preparedSession.Sections.TryGetValue(coordinate, out SectionTransferEntry? entry) ||
          entry.State is WorldSectionTransferState.NotScheduled or
              WorldSectionTransferState.Captured).ToArray();
      int pending = preparedSession.Sections.Values.Count(entry =>
          entry.State != WorldSectionTransferState.LocallySent);
      int additional = required.Count(coordinate => !preparedSession.Sections.ContainsKey(coordinate));
      if (required.Length > _options.MaximumRequestBatchSections ||
          pending + additional > _options.MaximumPendingSections ||
          _nextQueueOrder > long.MaxValue - needsBaseline.Length) {
        status = WorldSectionTransferStatus.BudgetExceeded;
        return false;
      }

      // Preserve the initial-plan barrier if admission has not captured its baseline yet.
      if (needsBaseline.Any(coordinate => preparedSession.InitialSectionSet.Contains(coordinate) &&
          preparedSession.Sections.TryGetValue(coordinate, out SectionTransferEntry? entry) &&
          entry.State == WorldSectionTransferState.NotScheduled) &&
          capturedHeader.Version != token.PlanVersion) {
        status = WorldSectionTransferStatus.StalePlan;
        return false;
      }
      WorldSectionSnapshot[] baselines = [];
      if (needsBaseline.Length > 0) {
        WorldSectionSnapshotBatch batch;
        try {
          batch = _world.CaptureSections(needsBaseline);
        } catch (InvalidOperationException) {
          status = WorldSectionTransferStatus.StaleWorld;
          return false;
        }
        if (batch.WorldEpoch != capturedHeader.WorldEpoch || batch.Version != capturedHeader.Version) {
          status = WorldSectionTransferStatus.StaleVersion;
          return false;
        }
        baselines = batch.Sections.ToArray();
      }

      Dictionary<WorldSectionCoordinate, SectionTransferEntry> original = preparedSession.Sections;
      Dictionary<WorldSectionCoordinate, SectionTransferEntry> expected =
          original.ToDictionary(pair => pair.Key, pair => CopySectionEntry(pair.Value));
      Dictionary<WorldSectionCoordinate, SectionTransferEntry> staged =
          original.ToDictionary(pair => pair.Key, pair => CopySectionEntry(pair.Value));
      long expectedQueueOrder = _nextQueueOrder;
      long stagedQueueOrder = expectedQueueOrder;
      foreach (WorldSectionSnapshot baseline in baselines) {
        if (!staged.TryGetValue(baseline.Coordinate, out SectionTransferEntry? entry)) {
          entry = new SectionTransferEntry();
          staged.Add(baseline.Coordinate, entry);
        }
        entry.Snapshot = baseline;
        entry.State = WorldSectionTransferState.Queued;
        entry.QueueOrder = ++stagedQueueOrder;
      }

      preparation = new WorldTeamSpawnPreparation(
          token, team, capturedHeader.Version, spawn, required, baselines,
          ValidatePreparation, CommitPreparation);
      status = WorldSectionTransferStatus.Accepted;
      return true;

      WorldSectionTransferStatus ValidatePreparation() {
        lock (_gate) {
          if (!TryReadPreparationSession(token, out TransferSession? current,
              out WorldHeaderCapture? currentHeader, out WorldSectionTransferStatus failure)) {
            return failure;
          }
          if (currentHeader!.Version != capturedHeader.Version) {
            return WorldSectionTransferStatus.StaleVersion;
          }
          if (!ReferenceEquals(current, preparedSession) ||
              !ReferenceEquals(current!.Sections, original) ||
              !ReferenceEquals(currentHeader.Header.TeamSpawns, facts) ||
              _nextQueueOrder != expectedQueueOrder || original.Count != expected.Count) {
            return WorldSectionTransferStatus.StalePlan;
          }
          foreach (var pair in expected) {
            if (!original.TryGetValue(pair.Key, out SectionTransferEntry? entry) ||
                entry.State != pair.Value.State ||
                !ReferenceEquals(entry.Snapshot, pair.Value.Snapshot) ||
                entry.PendingDeltaVersion != pair.Value.PendingDeltaVersion ||
                entry.QueueOrder != pair.Value.QueueOrder) {
              return WorldSectionTransferStatus.StalePlan;
            }
          }
          return WorldSectionTransferStatus.Accepted;
        }
      }

      void CommitPreparation() {
        lock (_gate) {
          preparedSession.Sections = staged;
          _nextQueueOrder = stagedQueueOrder;
        }
      }
    }
  }

  private bool TryReadPreparationSession(
      WorldSectionTransferToken token, out TransferSession? session,
      out WorldHeaderCapture? header, out WorldSectionTransferStatus status) {
    session = null;
    header = null;
    status = WorldSectionTransferStatus.InvalidInput;
    if (!token.IsValid) {
      return false;
    }
    if (!_sessions.TryGetValue(token.TransferId, out session) || session.Token != token) {
      status = WorldSectionTransferStatus.InvalidSession;
      return false;
    }
    if (session.Invalidated) {
      status = WorldSectionTransferStatus.SessionInvalidated;
      return false;
    }
    if (!TryCaptureCurrentHeader(out header) || header!.WorldEpoch != token.WorldEpoch) {
      status = WorldSectionTransferStatus.StaleWorld;
      return false;
    }
    status = WorldSectionTransferStatus.Accepted;
    return true;
  }

  private static SectionTransferEntry CopySectionEntry(SectionTransferEntry entry) {
    return new SectionTransferEntry {
      State = entry.State,
      Snapshot = entry.Snapshot,
      PendingDeltaVersion = entry.PendingDeltaVersion,
      QueueOrder = entry.QueueOrder
    };
  }
}
