using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public sealed partial class WorldSectionRequestCoordinator {
  private const int _sectionRequestBatchSize = 32;
  private readonly Dictionary<SessionEpoch, WorldSectionTransferToken> _transferTokens = [];
  private readonly HashSet<SessionEpoch> _completedInitialSync = [];
  private WorldSectionTransferCoordinator? _transferCoordinator;

  private WorldSectionTransferCoordinator _transfers =>
    _transferCoordinator ??= new WorldSectionTransferCoordinator(_world);

  /// <summary>
  /// Captures tiles, attachments and encoded fragments together on the server commit loop.
  /// P03 owns capture/state; Host supplies the source-backed packet-8 region and transfer identity.
  /// </summary>
  internal WorldSectionSendBatch CaptureInitialBaseline(
      SessionEpoch session,
      Guid transferId,
      WorldSnapshot world,
      TileCoordinate? playerSpawn,
      IReadOnlyCollection<TileCoordinate> additionalSpawns,
      IReadOnlyCollection<WorldSectionCoordinate> portalSections) {
    if (_transferTokens.ContainsKey(session) ||
        !_world.IsCurrentWorldRevision(world.WorldEpoch, world.Version)) {
      throw new InvalidOperationException("The initial section transfer is stale or already open.");
    }

    IReadOnlyList<WorldSectionCoordinate> sections = PlanInitialSections(
      world, playerSpawn, additionalSpawns, portalSections);
    // P03's initial plan seeds the world-spawn neighborhood. Schedule the remaining Version4
    // region through its existing bounded section-request API before capturing any baseline.
    WorldSectionPlan plan = _world.PlanInitialSections(null, [], []);
    WorldSectionTransferStartResult started = _transfers.StartSession(session, plan, transferId);
    if (!started.Accepted) {
      throw new InvalidOperationException($"Initial section planning ended with {started.Status}.");
    }

    _transferTokens.Add(session, started.Token);
    try {
      if (BeginWorld(session, world) != WorldSectionDeliveryResult.Queued) {
        throw new InvalidOperationException("The section visibility ledger rejected the world.");
      }

      foreach (WorldSectionCoordinate[] chunk in sections.Chunk(_sectionRequestBatchSize)) {
        WorldSection159RequestBatchResult scheduled = _transfers.HandleRequestSection159Batch(
          started.Token,
          chunk.Select(section => (checked((ushort)section.X), checked((ushort)section.Y)))
            .ToArray());
        if (scheduled.Status is not (WorldSectionTransferStatus.Accepted or
            WorldSectionTransferStatus.AlreadyScheduled)) {
          throw new InvalidOperationException($"Initial section scheduling ended with {scheduled.Status}.");
        }
      }

      var groups = new List<WorldSectionSendBatch.Section>(sections.Count);
      foreach (WorldSectionCoordinate[] chunk in sections.Chunk(_sectionRequestBatchSize)) {
        WorldSectionTransferCaptureResult captured = _transfers.CaptureBaseline(started.Token, chunk);
        RequireCapture(captured, chunk.Length);
        foreach (WorldSectionSnapshot snapshot in captured.Snapshots) {
          if (snapshot.WorldEpoch != world.WorldEpoch || snapshot.Version != world.Version) {
            throw new InvalidOperationException("The initial section capture changed world revision.");
          }

          groups.Add(EncodeSection(session, snapshot));
        }
      }

      return new WorldSectionSendBatch(started.Token, groups);
    } catch {
      RetireSession(session);
      throw;
    }
  }

  internal WorldSectionSendBatch CaptureRequestedBaseline(
      SessionEpoch session, ushort sectionX, ushort sectionY) {
    if (!HasCompletedInitialSync(session) ||
        !_transferTokens.TryGetValue(session, out WorldSectionTransferToken token)) {
      throw new InvalidOperationException("The session has no completed initial section transfer.");
    }

    var coordinate = new WorldSectionCoordinate(sectionX, sectionY);
    WorldSection159RequestResult scheduled = _transfers.HandleRequestSection159(
      token, sectionX, sectionY);
    if (scheduled.Status is not (WorldSectionTransferStatus.Accepted or
        WorldSectionTransferStatus.AlreadyScheduled)) {
      throw new InvalidOperationException($"Section request ended with {scheduled.Status}.");
    }

    if (_ledger.HasQueuedSection(session, token.WorldEpoch, coordinate)) {
      return new WorldSectionSendBatch(token, []);
    }

    WorldSectionTransferCaptureResult captured = _transfers.CaptureBaseline(token, [coordinate]);
    RequireCapture(captured, 1);
    return new WorldSectionSendBatch(token, [EncodeSection(session, captured.Snapshots[0])]);
  }

  /// <summary>Captures only unseen sections around an accepted player position.</summary>
  internal WorldSectionSendBatch? CaptureMovementBaseline(
      SessionEpoch session, float pixelX, float pixelY) {
    if (!HasCompletedInitialSync(session)) {
      return null;
    }

    WorldSectionTransferToken token = _transferTokens[session];
    WorldHeaderCapture header = _world.CaptureWorldHeader();
    WorldSectionCoordinate[] sections = header.Header.Geometry
      .GetSectionsAroundPixelPosition(pixelX, pixelY, radius: 1)
      .Where(section => !_ledger.HasQueuedSection(session, token.WorldEpoch, section))
      .ToArray();
    if (sections.Length == 0) {
      return null;
    }

    try {
      WorldSection159RequestBatchResult scheduled = _transfers.HandleRequestSection159Batch(
        token, sections.Select(section =>
          (checked((ushort)section.X), checked((ushort)section.Y))).ToArray());
      if (scheduled.Status is not (WorldSectionTransferStatus.Accepted or
          WorldSectionTransferStatus.AlreadyScheduled)) {
        throw new InvalidOperationException(
          $"Movement section scheduling ended with {scheduled.Status}.");
      }

      WorldSectionTransferCaptureResult captured = _transfers.CaptureBaseline(token, sections);
      RequireCapture(captured, sections.Length);
      return new WorldSectionSendBatch(token,
        captured.Snapshots.Select(snapshot => EncodeSection(session, snapshot)).ToArray());
    } catch {
      RetireTransfer(token);
      throw;
    }
  }

  /// <summary>Settles a complete fragment group on the commit loop after transport submission.</summary>
  internal bool RecordDelivery(
      WorldSectionSendBatch batch,
      WorldSectionSendBatch.Section section,
      IReadOnlyList<SendReceipt> receipts) {
    SessionEpoch session = batch.Token.SessionEpoch;
    if (!IsCurrentTransfer(batch.Token) || receipts.Count != section.Fragments.Count ||
        !batch.Sections.Contains(section)) {
      RetireTransfer(batch.Token);
      return false;
    }

    WorldSectionDeliveryResult result = _ledger.RecordPacket10Delivery(
      session, batch.Token.WorldEpoch, section.Coordinate, section.Version, receipts);
    if (result is not (WorldSectionDeliveryResult.Queued or WorldSectionDeliveryResult.LocallySent)) {
      RetireTransfer(batch.Token);
      return false;
    }

    WorldSectionTransferStatus queued = _transfers.RecordSendOutcome(
      batch.Token, section.Coordinate, WorldSectionTransferSendOutcome.Queued);
    if (queued is not (WorldSectionTransferStatus.Queued or WorldSectionTransferStatus.LocallySent)) {
      RetireTransfer(batch.Token);
      return false;
    }

    if (result == WorldSectionDeliveryResult.LocallySent &&
        _transfers.RecordSendOutcome(batch.Token, section.Coordinate,
          WorldSectionTransferSendOutcome.LocallySent) != WorldSectionTransferStatus.LocallySent) {
      RetireTransfer(batch.Token);
      return false;
    }

    return true;
  }

  internal bool CompleteInitialSync(WorldSectionSendBatch batch) {
    if (!IsCurrentTransfer(batch.Token) || batch.Sections.Count == 0 ||
        batch.Sections.Any(section => !_ledger.HasQueuedSection(
          batch.Token.SessionEpoch, batch.Token.WorldEpoch, section.Coordinate))) {
      return false;
    }

    _completedInitialSync.Add(batch.Token.SessionEpoch);
    return true;
  }

  internal bool HasCompletedInitialSync(SessionEpoch session) {
    return _completedInitialSync.Contains(session) &&
      _transferTokens.TryGetValue(session, out WorldSectionTransferToken token) &&
      IsCurrentTransfer(token);
  }

  internal void RetireTransfer(WorldSectionTransferToken token) {
    if (_transferTokens.TryGetValue(token.SessionEpoch, out WorldSectionTransferToken current) &&
        current == token) {
      RetireSession(token.SessionEpoch);
    }
  }

  internal bool IsCurrentTransfer(WorldSectionTransferToken token) {
    return _transferTokens.TryGetValue(token.SessionEpoch, out WorldSectionTransferToken current) &&
      current == token && _transfers.IsSessionValid(token);
  }

  private WorldSectionSendBatch.Section EncodeSection(
      SessionEpoch session, WorldSectionSnapshot snapshot) {
    return EncodeSection(session, snapshot, out _);
  }

  private WorldSectionSendBatch.Section EncodeSection(
      SessionEpoch session, WorldSectionSnapshot snapshot,
      out IReadOnlyList<TileSectionPacket> typedPackets) {
    WorldSectionPacketAttachments attachments = _captureAttachments.Invoke(snapshot);
    IReadOnlyList<Version4EncodedPacket> encoded =
      _encoder.EncodeSection(snapshot, attachments, out typedPackets);
    OutboundPacketIntent[] fragments = encoded.Select(packet => new OutboundPacketIntent(
      [session], null, packet.MessageId, packet.Payload, snapshot.Version)).ToArray();
    return new WorldSectionSendBatch.Section(snapshot.Coordinate, snapshot.Version,
      Array.AsReadOnly(fragments));
  }

  private static void RequireCapture(WorldSectionTransferCaptureResult captured, int count) {
    if (captured.Status is not (WorldSectionTransferStatus.Captured or
        WorldSectionTransferStatus.Accepted) || captured.Snapshots.Count != count) {
      throw new InvalidOperationException($"Section capture ended with {captured.Status}.");
    }
  }
}
