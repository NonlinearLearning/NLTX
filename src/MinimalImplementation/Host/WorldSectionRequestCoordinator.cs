using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>Plans and sends one complete section baseline before publishing visibility.</summary>
public sealed partial class WorldSectionRequestCoordinator
{
  private readonly WorldOwner _world;
  private readonly WorldSectionPacketEncoder _encoder;
  private readonly WorldSectionDeliveryLedger _ledger;
  private readonly Func<WorldSectionSnapshot, WorldSectionPacketAttachments> _captureAttachments;

  public WorldSectionRequestCoordinator(
    WorldOwner world,
    WorldSectionPacketEncoder encoder,
    WorldSectionDeliveryLedger ledger,
    Func<WorldSectionSnapshot, WorldSectionPacketAttachments> captureAttachments)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(encoder);
    ArgumentNullException.ThrowIfNull(ledger);
    ArgumentNullException.ThrowIfNull(captureAttachments);
    _world = world;
    _encoder = encoder;
    _ledger = ledger;
    _captureAttachments = captureAttachments;
  }

  public WorldSectionDeliveryResult BeginWorld(SessionEpoch session, WorldSnapshot world)
  {
    ArgumentNullException.ThrowIfNull(world);
    return _ledger.BeginWorld(session, world.WorldEpoch, world.Header.Geometry)
      ? WorldSectionDeliveryResult.Queued
      : WorldSectionDeliveryResult.InvalidInput;
  }

  public IReadOnlyList<WorldSectionCoordinate> PlanInitialSections(
    WorldSnapshot world,
    TileCoordinate? playerSpawn,
    IReadOnlyCollection<TileCoordinate> additionalSpawns,
    IReadOnlyCollection<WorldSectionCoordinate> portalSections)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(additionalSpawns);
    ArgumentNullException.ThrowIfNull(portalSections);
    WorldGeometry geometry = world.Header.Geometry;
    var result = new HashSet<WorldSectionCoordinate>();
    AddSpawnRegion(result, geometry, world.Header.Spawn, width: 5, height: 3);
    if (playerSpawn is TileCoordinate selectedSpawn && geometry.Contains(selectedSpawn))
    {
      AddSpawnRegion(result, geometry, selectedSpawn, width: 6, height: 4);
    }

    foreach (TileCoordinate spawn in additionalSpawns)
    {
      if (geometry.Contains(spawn))
      {
        AddSpawnRegion(result, geometry, spawn, width: 6, height: 4);
      }
    }

    foreach (WorldSectionCoordinate section in portalSections)
    {
      if (!geometry.ContainsSection(section))
      {
        throw new ArgumentOutOfRangeException(nameof(portalSections));
      }

      result.Add(section);
    }

    return Array.AsReadOnly(result
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray());
  }

  public async Task<WorldSectionDeliveryResult> SendSectionAsync(
    SessionEpoch session,
    ulong worldEpoch,
    WorldSectionCoordinate coordinate,
    Func<OutboundPacketIntent, CancellationToken, ValueTask<SendReceipt?>> send,
    CancellationToken cancellationToken = default,
    WorldSectionPacketAttachments? capturedAttachments = null)
  {
    ArgumentNullException.ThrowIfNull(send);
    if (!_world.IsCurrentWorldEpoch(worldEpoch))
    {
      return WorldSectionDeliveryResult.StaleWorld;
    }

    WorldSectionSnapshot section;
    WorldSectionPacketAttachments attachments;
    IReadOnlyList<Version4EncodedPacket> packets;
    try
    {
      section = _world.CaptureSection(coordinate);
      attachments = capturedAttachments ?? _captureAttachments(section);
      if (attachments.Version != section.Version)
      {
        return WorldSectionDeliveryResult.StaleWorld;
      }
      packets = _encoder.EncodeSection(section, attachments);
    }
    catch (ArgumentOutOfRangeException)
    {
      return WorldSectionDeliveryResult.InvalidSection;
    }

    if (!_world.IsCurrentWorldEpoch(worldEpoch))
    {
      return WorldSectionDeliveryResult.StaleWorld;
    }

    var receipts = new List<SendReceipt>(packets.Count);
    foreach (var packet in packets)
    {
      cancellationToken.ThrowIfCancellationRequested();
      var intent = new OutboundPacketIntent(
        [session],
        ignoreEpoch: null,
        packet.MessageId,
        packet.Payload,
        section.Version);
      SendReceipt? receipt = await send(intent, cancellationToken).ConfigureAwait(false);
      if (receipt is null)
      {
        if (receipts.Count != 0)
        {
          _ledger.RemoveSession(session);
          return WorldSectionDeliveryResult.SessionInvalidated;
        }

        return WorldSectionDeliveryResult.NotSubmitted;
      }

      receipts.Add(receipt);
      if (receipt.Certainty is TransportSendCertainty.Failed or
          TransportSendCertainty.OutcomeUnknown)
      {
        return _ledger.RecordPacket10Delivery(
          session,
          worldEpoch,
          coordinate,
          section.Version,
          receipts);
      }

      if (receipt.Certainty == TransportSendCertainty.NotSubmitted)
      {
        return _ledger.RecordPacket10Delivery(
          session,
          worldEpoch,
          coordinate,
          section.Version,
          receipts);
      }
    }

    return _ledger.RecordPacket10Delivery(session, worldEpoch, coordinate, section.Version, receipts);
  }

  public bool HasQueuedSection(SessionEpoch session, ulong worldEpoch, WorldSectionCoordinate section)
  {
    return _ledger.HasQueuedSection(session, worldEpoch, section);
  }

  public void RetireSession(SessionEpoch session)
  {
    if (_transferTokens.Remove(session, out WorldSectionTransferToken token)) {
      _transfers.RetireSession(token);
    }

    _completedInitialSync.Remove(session);
    _ledger.RemoveSession(session);
  }

  private static void AddSpawnRegion(
    ISet<WorldSectionCoordinate> sections,
    WorldGeometry geometry,
    TileCoordinate spawn,
    int width,
    int height)
  {
    // Version4 packet 8 uses an exclusive 5x3 world-spawn range, and inclusive
    // endpoints (6x4) for selected and team spawns. Offsets are section coordinates.
    int startX = spawn.X / WorldGeometry.SectionWidth - 2;
    int startY = spawn.Y / WorldGeometry.SectionHeight - 1;
    for (int y = Math.Max(0, startY); y < Math.Min(geometry.SectionCountY, startY + height); y++)
    {
      for (int x = Math.Max(0, startX); x < Math.Min(geometry.SectionCountX, startX + width); x++)
      {
        sections.Add(new WorldSectionCoordinate(x, y));
      }
    }
  }
}
