using System.Collections.Concurrent;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace NSSLC.Infrastructure.Network;

public sealed class WorldSynchronizationPacketHandlers : IPacketHandler<SpawnTileDataPacket>,
    IPacketHandler<RequestSectionPacket>, IPacketHandler<TileManipulationPacket>,
    IPacketHandler<SyncTilePickingPacket>, IPacketHandler<PlayerControlsPacket> {
  private const int SectionWidth = 200;
  private const int SectionHeight = 150;
  private readonly WorldDataPacket _worldData;
  private readonly TileMapSnapshot _tiles;
  private readonly IReadOnlyList<bool> _frameImportant;
  private readonly PlayerControlsObservationStore _controls;
  private readonly PlayerControlsPacketHandler _playerControls;
  private readonly Func<NetworkSessionContext, bool> _isCurrentSender;
  private readonly Func<EntityRuntimeId?>? _currentWorldRuntimeId;
  private readonly NetworkWorldItemOwner? _worldItems;
  private readonly TileBreakObservationStore _tileBreaks;
  private readonly object _tileGate = new();
  private readonly Dictionary<int, TileCellState> _tileChanges = new();
  private readonly Dictionary<byte, (ConnectionIdentity Connection,
      EntityRuntimeId WorldRuntimeId, HashSet<(int X, int Y)> Sections)> _transferredSections = new();
  private readonly ConcurrentDictionary<(int X, int Y), TileSectionPacket> _sectionCache = new();
  private readonly WorldSynchronizationObservation _observation;

  public WorldSynchronizationPacketHandlers(WorldDataPacket worldData, TileMapSnapshot tiles,
      IReadOnlyList<bool> frameImportant, PlayerControlsObservationStore controls,
      TileBreakObservationStore tileBreaks, WorldSynchronizationObservation observation,
      Func<NetworkSessionContext, bool> isCurrentSender,
      Func<EntityRuntimeId?>? currentWorldRuntimeId = null,
      NetworkWorldItemOwner? worldItems = null) {
    _worldData = worldData ?? throw new ArgumentNullException(nameof(worldData));
    _tiles = tiles ?? throw new ArgumentNullException(nameof(tiles));
    _frameImportant = frameImportant ?? throw new ArgumentNullException(nameof(frameImportant));
    _controls = controls ?? throw new ArgumentNullException(nameof(controls));
    _isCurrentSender = isCurrentSender
        ?? throw new ArgumentNullException(nameof(isCurrentSender));
    _currentWorldRuntimeId = currentWorldRuntimeId;
    _worldItems = worldItems;
    _playerControls = new PlayerControlsPacketHandler(controls, tiles.Width, tiles.Height);
    _tileBreaks = tileBreaks ?? throw new ArgumentNullException(nameof(tileBreaks));
    _observation = observation ?? throw new ArgumentNullException(nameof(observation));
    if (_tiles.Width != _worldData.MaxTilesX || _tiles.Height != _worldData.MaxTilesY) {
      throw new ArgumentException("The tile snapshot dimensions do not match packet 7.",
          nameof(tiles));
    }
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SpawnTileDataPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    PacketHandlingResult? invalidContext = ValidateContext(context,
        NetworkSessionStage.AwaitSectionRequest);
    if (invalidContext is not null) {
      return invalidContext;
    }
    if (!IsValidSpawnRequest(packet)) {
      return new PacketHandlingResult(false,
          rejectionCode: "InvalidSpawnTileRequest");
    }

    var requestedSections = new SortedSet<(int X, int Y)>();
    AddSpawnSections(requestedSections, _worldData.SpawnTileX, _worldData.SpawnTileY,
        inclusiveEnd: false);
    if (packet.X >= 0 && packet.Y >= 0) {
      AddSpawnSections(requestedSections, packet.X, packet.Y, inclusiveEnd: true);
    }

    List<(int X, int Y, TileSectionPacket Packet)> newSections =
        CaptureUntransferredSections(context, requestedSections);
    var outbound = new List<OutboundDispatch>(newSections.Count + 3) {
      new(_worldData, PacketDispatchKind.Single, [context.Connection],
          allowedStages: NetworkSessionStage.Synchronizing),
      new(new StatusTextSizePacket {
        Value = newSections.Count,
        Text = NetworkText.Literal("Receiving world tiles")
      }, PacketDispatchKind.Single, [context.Connection],
          allowedStages: NetworkSessionStage.Synchronizing)
    };
    var projectedItemSlots = new HashSet<short>();
    foreach ((int sectionX, int sectionY, TileSectionPacket section) in newSections) {
      outbound.Add(new OutboundDispatch(section, PacketDispatchKind.Single,
          [context.Connection], allowedStages: NetworkSessionStage.Synchronizing));
      await AppendSectionItemDispatchesAsync(context, sectionX, sectionY,
          NetworkSessionStage.Synchronizing, projectedItemSlots, outbound, cancellationToken)
          .ConfigureAwait(false);
      _observation.RecordSection(sectionX, sectionY, "spawn");
    }
    outbound.Add(new OutboundDispatch(new InitialSpawnPacket(),
        PacketDispatchKind.Single, [context.Connection],
        allowedStages: NetworkSessionStage.Synchronizing));
    return new PacketHandlingResult(true, outbound,
        nextStage: NetworkSessionStage.Synchronizing,
        interest: CreateSectionInterestProjection(context));
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      RequestSectionPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    PacketHandlingResult? invalidContext = ValidateContext(context, NetworkSessionStage.Active);
    if (invalidContext is not null) {
      return invalidContext;
    }
    if (packet.SectionX >= GetSectionCount(_tiles.Width, SectionWidth)
        || packet.SectionY >= GetSectionCount(_tiles.Height, SectionHeight)) {
      return new PacketHandlingResult(false,
          rejectionCode: "SectionOutOfWorldBounds");
    }

    int sectionX = packet.SectionX;
    int sectionY = packet.SectionY;
    var outbound = new List<OutboundDispatch> {
      new OutboundDispatch(new StatusTextSizePacket {
        Value = 1,
        Text = NetworkText.Literal("Receiving world tiles")
      }, PacketDispatchKind.Single, [context.Connection]),
      CreateTileDispatch(context, sectionX, sectionY, NetworkSessionStage.Active)
    };
    await AppendSectionItemDispatchesAsync(context, sectionX, sectionY,
        NetworkSessionStage.Active, new HashSet<short>(), outbound, cancellationToken)
        .ConfigureAwait(false);
    _observation.RecordSection(sectionX, sectionY, "request");
    return new PacketHandlingResult(true, outbound,
        interest: CreateSectionInterestProjection(context));
  }

  private async ValueTask AppendSectionItemDispatchesAsync(
      NetworkSessionContext context,
      int sectionX,
      int sectionY,
      NetworkSessionStage allowedStage,
      HashSet<short> projectedItemSlots,
      List<OutboundDispatch> outbound,
      CancellationToken cancellationToken) {
    if (_worldItems is null) {
      return;
    }

    IReadOnlyList<NetworkWorldItemPositionProjection> projections =
        await _worldItems.CaptureSectionItemsAsync(sectionX, sectionY, cancellationToken)
            .ConfigureAwait(false);
    outbound.AddRange(CreateSectionItemDispatches(context, projections, allowedStage,
        projectedItemSlots));
  }

  private static IReadOnlyList<OutboundDispatch> CreateSectionItemDispatches(
      NetworkSessionContext context,
      IReadOnlyList<NetworkWorldItemPositionProjection> projections,
      NetworkSessionStage allowedStage,
      HashSet<short> projectedItemSlots) {
    var dispatches = new List<OutboundDispatch>(projections.Count);
    foreach (NetworkWorldItemPositionProjection projection in projections) {
      if (!projectedItemSlots.Add(projection.ItemIndex)) {
        continue;
      }

      OutboundDispatch dispatch = SteamItemPacketCommandOwnerAdapter
          .ToSteamItemPositionDispatch(projection, context.Connection);
      dispatches.Add(new OutboundDispatch(dispatch.Packet, dispatch.Kind, dispatch.Targets,
          allowedStages: allowedStage));
    }

    return dispatches.AsReadOnly();
  }

  /// <summary>
  /// Creates the section projection used by packet 157's team-based spawn response.
  /// Section transfer state is shared with ordinary spawn, movement and explicit section
  /// requests so the same connection never receives a duplicate section unnecessarily.
  /// </summary>
  public IReadOnlyList<OutboundDispatch> CreateExtraSpawnSectionDispatches(
      NetworkSessionContext context,
      int tileX,
      int tileY) {
    if ((context.Stage & NetworkSessionStage.Active) == 0 ||
        context.WorldRuntimeId is null || !_isCurrentSender(context) ||
        (uint)tileX >= _tiles.Width || (uint)tileY >= _tiles.Height) {
      return Array.Empty<OutboundDispatch>();
    }

    int sectionX = tileX / SectionWidth;
    int sectionY = tileY / SectionHeight;
    lock (_tileGate) {
      HashSet<(int X, int Y)> transferred = GetTransferredSections(context);
      if (transferred.Contains((sectionX, sectionY))) {
        return Array.Empty<OutboundDispatch>();
      }

      return [
        new OutboundDispatch(
            new StatusTextSizePacket {
              Value = 1,
              Text = NetworkText.Literal("Receiving world tiles")
            },
            PacketDispatchKind.Single,
            [context.Connection],
            allowedStages: NetworkSessionStage.Active),
        CreateTileDispatch(context, sectionX, sectionY, NetworkSessionStage.Active)
      ];
    }
  }

  public async ValueTask<IReadOnlyList<OutboundDispatch>> CreateExtraSpawnSectionDispatchesAsync(
      NetworkSessionContext context,
      int tileX,
      int tileY,
      CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (_worldItems is null) {
      return CreateExtraSpawnSectionDispatches(context, tileX, tileY);
    }

    if ((context.Stage & NetworkSessionStage.Active) == 0 ||
        context.WorldRuntimeId is null || !_isCurrentSender(context) ||
        (uint)tileX >= _tiles.Width || (uint)tileY >= _tiles.Height) {
      return Array.Empty<OutboundDispatch>();
    }

    int sectionX = tileX / SectionWidth;
    int sectionY = tileY / SectionHeight;
    lock (_tileGate) {
      if (GetTransferredSections(context).Contains((sectionX, sectionY))) {
        return Array.Empty<OutboundDispatch>();
      }
    }

    IReadOnlyList<NetworkWorldItemPositionProjection> itemProjections =
        await _worldItems.CaptureSectionItemsAsync(sectionX, sectionY, cancellationToken)
            .ConfigureAwait(false);
    cancellationToken.ThrowIfCancellationRequested();
    IReadOnlyList<OutboundDispatch> itemDispatches = CreateSectionItemDispatches(
        context, itemProjections, NetworkSessionStage.Active, new HashSet<short>());
    IReadOnlyList<OutboundDispatch> sectionDispatches =
        CreateExtraSpawnSectionDispatches(context, tileX, tileY);
    if (sectionDispatches.Count == 0) {
      return sectionDispatches;
    }

    var outbound = new List<OutboundDispatch>(
        sectionDispatches.Count + itemDispatches.Count);
    outbound.AddRange(sectionDispatches);
    outbound.AddRange(itemDispatches);
    return outbound.AsReadOnly();
  }

  /// <summary>
  /// Captures the same section history used by packet 8, movement, portal spawns, and
  /// explicit section requests so callers can publish the updated session interest.
  /// </summary>
  public SectionInterestProjection CreateSectionInterestProjection(
      NetworkSessionContext context) {
    if ((context.Stage & (NetworkSessionStage.AwaitSectionRequest
            | NetworkSessionStage.Synchronizing | NetworkSessionStage.Active)) == 0
        || context.WorldRuntimeId is null || !_isCurrentSender(context)) {
      throw new ArgumentException("A current world session is required.", nameof(context));
    }
    lock (_tileGate) {
      HashSet<(int X, int Y)> transferred = GetTransferredSections(context);
      return new SectionInterestProjection(context.WorldRuntimeId.Value.Value, 0,
          transferred.Count,
          transferred.Select(section =>
              new Terraria.Network.SectionCoordinate(section.X, section.Y)));
    }
  }

  /// <summary>
  /// Projects one explicit server-side frame repair into packet 11 section dispatches.
  /// Bounds are inclusive tile coordinates returned by the framing effect owner. Partial
  /// overlap with the world is clipped; a region outside the world produces no dispatch.
  /// Each affected section gets one packet 11 routed to subscribers of that section.
  /// </summary>
  public TileFrameRepairDispatchResult CreateFrameRepairDispatches(
      EntityRuntimeId eventWorldRuntimeId,
      int startTileX,
      int startTileY,
      int endTileXInclusive,
      int endTileYInclusive) {
    if (!eventWorldRuntimeId.IsAssigned || _currentWorldRuntimeId is null
        || _currentWorldRuntimeId() != eventWorldRuntimeId) {
      return TileFrameRepairDispatchResult.Rejected("StaleWorldRuntime");
    }
    if (startTileX > endTileXInclusive || startTileY > endTileYInclusive) {
      return TileFrameRepairDispatchResult.Rejected("InvalidFrameRepairBounds");
    }

    int clippedStartX = Math.Max(0, startTileX);
    int clippedStartY = Math.Max(0, startTileY);
    int clippedEndX = Math.Min(_tiles.Width - 1, endTileXInclusive);
    int clippedEndY = Math.Min(_tiles.Height - 1, endTileYInclusive);
    if (clippedStartX > clippedEndX || clippedStartY > clippedEndY) {
      return TileFrameRepairDispatchResult.Succeeded([]);
    }

    int firstSectionX = clippedStartX / SectionWidth;
    int firstSectionY = clippedStartY / SectionHeight;
    int lastSectionX = clippedEndX / SectionWidth;
    int lastSectionY = clippedEndY / SectionHeight;
    var outbound = new List<OutboundDispatch>(
        checked((lastSectionX - firstSectionX + 1) * (lastSectionY - firstSectionY + 1)));
    for (int sectionX = firstSectionX; sectionX <= lastSectionX; sectionX++) {
      for (int sectionY = firstSectionY; sectionY <= lastSectionY; sectionY++) {
        if (sectionX > short.MaxValue || sectionY > short.MaxValue) {
          return TileFrameRepairDispatchResult.Rejected("FrameRepairSectionOutOfRange");
        }
        var section = new Terraria.Network.SectionCoordinate(sectionX, sectionY);
        outbound.Add(new OutboundDispatch(new TileFrameSectionPacket {
          X = (short)sectionX,
          Y = (short)sectionY,
          Width = (short)sectionX,
          Height = (short)sectionY
        }, PacketDispatchKind.SectionSubscribers,
            allowedStages: NetworkSessionStage.Active,
            worldKey: eventWorldRuntimeId.Value,
            worldGeneration: 0,
            section: section));
      }
    }
    return TileFrameRepairDispatchResult.Succeeded(outbound);
  }

  /// <summary>
  /// Sends an explicit frame repair through the Gateway's server-originated world path.
  /// </summary>
  public async ValueTask<TileFrameRepairPublicationResult> PublishFrameRepairAsync(
      PacketGateway gateway,
      EntityRuntimeId eventWorldRuntimeId,
      int startTileX,
      int startTileY,
      int endTileXInclusive,
      int endTileYInclusive,
      CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(gateway);
    TileFrameRepairDispatchResult repair = CreateFrameRepairDispatches(
        eventWorldRuntimeId, startTileX, startTileY,
        endTileXInclusive, endTileYInclusive);
    if (!repair.Accepted) {
      return TileFrameRepairPublicationResult.Rejected(repair.RejectionCode!, 0);
    }

    int publishedDispatches = 0;
    foreach (OutboundDispatch dispatch in repair.Outbound) {
      cancellationToken.ThrowIfCancellationRequested();
      if (!await gateway.PublishWorldAsync(eventWorldRuntimeId, dispatch,
          cancellationToken).ConfigureAwait(false)) {
        return TileFrameRepairPublicationResult.Rejected(
            "StaleWorldRuntime", publishedDispatches);
      }
      publishedDispatches++;
    }
    return TileFrameRepairPublicationResult.Succeeded(publishedDispatches);
  }

  public async ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      PlayerControlsPacket packet, CancellationToken cancellationToken) {
    PacketHandlingResult? invalidContext = ValidateContext(context, NetworkSessionStage.Active);
    if (invalidContext is not null) {
      return invalidContext;
    }
    PacketHandlingResult controls = await _playerControls.HandleAsync(context, packet,
        cancellationToken).ConfigureAwait(false);
    if (!controls.Accepted) {
      return controls;
    }

    int sectionX = (int)(packet.Position.X / 16f) / SectionWidth;
    int sectionY = (int)(packet.Position.Y / 16f) / SectionHeight;
    var outbound = new List<OutboundDispatch>();
    var newlyTransferredSections = new List<(int X, int Y)>();
    lock (_tileGate) {
      HashSet<(int X, int Y)> transferred = GetTransferredSections(context);
      for (int x = Math.Max(0, sectionX - 1);
          x <= Math.Min(GetSectionCount(_tiles.Width, SectionWidth) - 1, sectionX + 1); x++) {
        for (int y = Math.Max(0, sectionY - 1);
            y <= Math.Min(GetSectionCount(_tiles.Height, SectionHeight) - 1, sectionY + 1); y++) {
          if (!transferred.Contains((x, y))) {
            outbound.Add(CreateTileDispatch(context, x, y, NetworkSessionStage.Active));
            newlyTransferredSections.Add((x, y));
            _observation.RecordSection(x, y, "movement");
          }
        }
      }
    }
    if (outbound.Count > 0) {
      int transferredSectionCount = outbound.Count;
      outbound.Insert(0, new OutboundDispatch(new StatusTextSizePacket {
        Value = transferredSectionCount,
        Text = NetworkText.Literal("Receiving world tiles")
      }, PacketDispatchKind.Single, [context.Connection]));
    }
    var projectedItemSlots = new HashSet<short>();
    foreach ((int transferredSectionX, int transferredSectionY) in newlyTransferredSections) {
      await AppendSectionItemDispatchesAsync(context, transferredSectionX, transferredSectionY,
          NetworkSessionStage.Active, projectedItemSlots, outbound, cancellationToken)
          .ConfigureAwait(false);
    }
    return new PacketHandlingResult(true, outbound,
        interest: CreateSectionInterestProjection(context));
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      SyncTilePickingPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    PacketHandlingResult? invalidContext = ValidateContext(context, NetworkSessionStage.Active);
    if (invalidContext is not null) {
      return ValueTask.FromResult(invalidContext);
    }
    if ((uint)packet.X >= _tiles.Width || (uint)packet.Y >= _tiles.Height) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "TileOutsideWorld"));
    }
    // The field named TileType carries pick damage, not a tile ID. This is a visual notice
    // to other players; only packet 17 can change the headless host's terrain overlay.
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      TileManipulationPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    PacketHandlingResult? invalidContext = ValidateContext(context, NetworkSessionStage.Active);
    if (invalidContext is not null) {
      return ValueTask.FromResult(invalidContext);
    }
    if (packet.Action == 1) {
      return PlaceTile(context, packet);
    }
    if (packet.Action != 0 || packet.TileOrWallType is < 0 or > 1
        || !_controls.TryGetLastPosition(context.Actor.PlayerSlot,
            out PacketVector2 position)) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "UnsupportedTileManipulation"));
    }

    TileCellState before;
    TileCellState after;
    bool changed = false;
    lock (_tileGate) {
      if ((uint)packet.X >= _tiles.Width || (uint)packet.Y >= _tiles.Height) {
        return ValueTask.FromResult(new PacketHandlingResult(false,
            rejectionCode: "TileOutsideWorld"));
      }
      float dx = packet.X * 16f + 8f - position.X;
      float dy = packet.Y * 16f + 8f - position.Y;
      if (dx * dx + dy * dy > 160f * 160f) {
        return ValueTask.FromResult(new PacketHandlingResult(false,
            rejectionCode: "TileOutOfReach"));
      }

      // For action 0, the short field is the fail flag: 1 is a partial hit, not a break.
      if (packet.TileOrWallType == 1) {
        return ValueTask.FromResult(new PacketHandlingResult(true));
      }
      before = GetTile(packet.X, packet.Y);
      after = before;
      if ((before.TileHeader & 0x20) != 0 && before.Type < _frameImportant.Count
          && !_frameImportant[before.Type]) {
        after.Type = 0;
        after.FrameX = 0;
        after.FrameY = 0;
        after.TileHeader &= 0x8380;
        after.Header3 &= 0x60;
        _tileChanges[packet.X * _tiles.Height + packet.Y] = after;
        _sectionCache.TryRemove((packet.X / SectionWidth, packet.Y / SectionHeight), out _);
        changed = true;
      }
    }

    if (changed) {
      _tileBreaks.Record(context.Actor.PlayerSlot, packet.X, packet.Y, before.Type,
          (after.TileHeader & 0x20) != 0);
    }
    return CreateTileCorrection(context, packet, after);
  }

  private ValueTask<PacketHandlingResult> PlaceTile(NetworkSessionContext context,
      TileManipulationPacket packet) {
    if (packet.TileOrWallType < 0 || packet.TileOrWallType >= _frameImportant.Count) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "InvalidPlacementTileType"));
    }
    if ((uint)packet.X >= _tiles.Width || (uint)packet.Y >= _tiles.Height) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "TileOutsideWorld"));
    }
    if (!_controls.TryGetLastPosition(context.Actor.PlayerSlot, out PacketVector2 position)) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "MissingPlayerPosition"));
    }
    float dx = packet.X * 16f + 8f - position.X;
    float dy = packet.Y * 16f + 8f - position.Y;
    if (dx * dx + dy * dy > 160f * 160f) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "TileOutOfReach"));
    }

    TileCellState after;
    lock (_tileGate) {
      after = GetTile(packet.X, packet.Y);
      // The headless overlay supports ordinary single-cell blocks. Correct occupied cells
      // and framed objects back to the stored state without terminating a valid session.
      if ((after.TileHeader & 0x20) == 0 && !_frameImportant[packet.TileOrWallType]) {
        after.Type = (ushort)packet.TileOrWallType;
        after.FrameX = 0;
        after.FrameY = 0;
        after.TileHeader = (ushort)((after.TileHeader & 0x8380) | 0x20);
        after.Header3 &= 0x40;
        _tileChanges[packet.X * _tiles.Height + packet.Y] = after;
        _sectionCache.TryRemove((packet.X / SectionWidth, packet.Y / SectionHeight), out _);
      }
    }
    return CreateTileCorrection(context, packet, after);
  }

  private static ValueTask<PacketHandlingResult> CreateTileCorrection(
      NetworkSessionContext context, TileManipulationPacket packet, TileCellState tile) {
    var tileUpdate = new AreaTileChangePacket {
      StartX = packet.X,
      StartY = packet.Y,
      Width = 1,
      Height = 1,
      ChangeType = 0,
      Tiles = [ProjectAreaTile(tile)]
    };
    var dispatch = new OutboundDispatch(tileUpdate, PacketDispatchKind.Single,
        [context.Connection], allowedStages: NetworkSessionStage.Active);
    return ValueTask.FromResult(new PacketHandlingResult(true, [dispatch]));
  }

  private OutboundDispatch CreateTileDispatch(NetworkSessionContext context,
      int sectionX, int sectionY, NetworkSessionStage stage) {
    lock (_tileGate) {
      TileSectionPacket packet = _sectionCache.GetOrAdd((sectionX, sectionY),
          key => CreateTileSection(key.X, key.Y));
      GetTransferredSections(context).Add((sectionX, sectionY));
      return new OutboundDispatch(packet, PacketDispatchKind.Single, [context.Connection],
          allowedStages: stage);
    }
  }

  // Called under _tileGate. Slot reuse resets transfer state for the new connection;
  // retained state is bounded by the player-slot table and the world's section count.
  private HashSet<(int X, int Y)> GetTransferredSections(NetworkSessionContext context) {
    EntityRuntimeId worldRuntimeId = context.WorldRuntimeId
        ?? throw new InvalidOperationException("Section transfers require a world runtime.");
    byte slot = context.Actor.PlayerSlot;
    if (!_transferredSections.TryGetValue(slot, out var transferred)
        || transferred.Connection != context.Connection
        || transferred.WorldRuntimeId != worldRuntimeId) {
      transferred = (context.Connection, worldRuntimeId, new HashSet<(int X, int Y)>());
      _transferredSections[slot] = transferred;
    }
    return transferred.Sections;
  }

  private TileSectionPacket CreateTileSection(int sectionX, int sectionY) {
    int startX = checked(sectionX * SectionWidth);
    int startY = checked(sectionY * SectionHeight);
    int width = Math.Min(SectionWidth, _tiles.Width - startX);
    int height = Math.Min(SectionHeight, _tiles.Height - startY);
    var tiles = new Packet10Tile[checked(width * height)];
    int index = 0;
    for (int y = startY; y < startY + height; y++) {
      for (int x = startX; x < startX + width; x++) {
        tiles[index++] = ProjectTile(GetTile(x, y));
      }
    }
    return new TileSectionPacket {
      StartX = startX,
      StartY = startY,
      Width = checked((short)width),
      Height = checked((short)height),
      Tiles = tiles
    };
  }

  private void AddSpawnSections(SortedSet<(int X, int Y)> sections, int tileX, int tileY,
      bool inclusiveEnd) {
    int startX = tileX / SectionWidth - 2;
    int startY = tileY / SectionHeight - 1;
    int endX = startX + 5;
    int endY = startY + 3;
    int sectionCountX = GetSectionCount(_tiles.Width, SectionWidth);
    int sectionCountY = GetSectionCount(_tiles.Height, SectionHeight);
    int firstX = Math.Max(0, startX);
    int firstY = Math.Max(0, startY);
    int lastX = inclusiveEnd
        ? Math.Min(sectionCountX - 1, endX)
        : Math.Min(sectionCountX - 1, endX - 1);
    int lastY = inclusiveEnd
        ? Math.Min(sectionCountY - 1, endY)
        : Math.Min(sectionCountY - 1, endY - 1);
    for (int x = firstX; x <= lastX; x++) {
      for (int y = firstY; y <= lastY; y++) {
        sections.Add((x, y));
      }
    }
  }

  private bool IsValidSpawnRequest(SpawnTileDataPacket packet) {
    bool defaultSpawn = packet.X == -1 && packet.Y == -1;
    bool selectedSpawn = packet.X >= 10 && packet.X <= _tiles.Width - 10
        && packet.Y >= 10 && packet.Y <= _tiles.Height - 10;
    return packet.Team < 6 && (defaultSpawn || selectedSpawn);
  }

  private PacketHandlingResult? ValidateContext(NetworkSessionContext context,
      NetworkSessionStage allowedStage) {
    if ((context.Stage & allowedStage) == 0) {
      return new PacketHandlingResult(false, rejectionCode: "InvalidWorldSessionStage");
    }
    if (context.WorldRuntimeId is null || !_isCurrentSender(context)) {
      return new PacketHandlingResult(false, rejectionCode: "StaleWorldSender");
    }
    return null;
  }

  private List<(int X, int Y, TileSectionPacket Packet)> CaptureUntransferredSections(
      NetworkSessionContext context, IReadOnlyCollection<(int X, int Y)> requestedSections) {
    lock (_tileGate) {
      HashSet<(int X, int Y)> transferred = GetTransferredSections(context);
      var sections = new List<(int X, int Y, TileSectionPacket Packet)>();
      foreach ((int sectionX, int sectionY) in requestedSections) {
        if (transferred.Contains((sectionX, sectionY))) {
          continue;
        }
        TileSectionPacket packet = _sectionCache.GetOrAdd((sectionX, sectionY),
            key => CreateTileSection(key.X, key.Y));
        sections.Add((sectionX, sectionY, packet));
      }
      foreach (var section in sections) {
        transferred.Add((section.X, section.Y));
      }
      return sections;
    }
  }

  private static int GetSectionCount(int dimension, int sectionSize) =>
      (dimension + sectionSize - 1) / sectionSize;

  private TileCellState GetTile(int x, int y) {
    int index = x * _tiles.Height + y;
    return _tileChanges.TryGetValue(index, out TileCellState changed)
        ? changed
        : _tiles.GetTile(x, y);
  }

  private static Packet10Tile ProjectTile(TileCellState tile) {
    ushort tileHeader = tile.TileHeader;
    return new Packet10Tile {
      Active = (tileHeader & 0x20) != 0,
      Type = tile.Type,
      FrameX = tile.FrameX,
      FrameY = tile.FrameY,
      TileColor = (byte)(tileHeader & 0x1f),
      Wall = tile.Wall,
      WallColor = (byte)(tile.Header & 0x1f),
      LiquidAmount = tile.LiquidAmount,
      LiquidType = (Packet10LiquidType)tile.LiquidType,
      Wire = (tileHeader & 0x80) != 0,
      Wire2 = (tileHeader & 0x100) != 0,
      Wire3 = (tileHeader & 0x200) != 0,
      Wire4 = (tile.Header & 0x80) != 0,
      HalfBrick = (tileHeader & 0x400) != 0,
      Slope = (byte)((tileHeader >> 12) & 0x07),
      Actuator = (tileHeader & 0x800) != 0,
      Inactive = (tileHeader & 0x40) != 0,
      InvisibleBlock = (tile.Header3 & 0x20) != 0,
      InvisibleWall = (tile.Header3 & 0x40) != 0,
      FullbrightBlock = (tile.Header3 & 0x80) != 0,
      FullbrightWall = (tileHeader & 0x8000) != 0
    };
  }

  private static Packet20Tile ProjectAreaTile(TileCellState tile) {
    ushort tileHeader = tile.TileHeader;
    return new Packet20Tile {
      Active = (tileHeader & 0x20) != 0,
      TileType = tile.Type,
      FrameX = tile.FrameX,
      FrameY = tile.FrameY,
      TileColor = (byte)(tileHeader & 0x1f),
      Wall = tile.Wall,
      WallColor = (byte)(tile.Header & 0x1f),
      LiquidAmount = tile.LiquidAmount,
      LiquidType = tile.LiquidType,
      Wire = (tileHeader & 0x80) != 0,
      Wire2 = (tileHeader & 0x100) != 0,
      Wire3 = (tileHeader & 0x200) != 0,
      Wire4 = (tile.Header & 0x80) != 0,
      HalfBrick = (tileHeader & 0x400) != 0,
      Slope = (byte)((tileHeader >> 12) & 0x07),
      Actuator = (tileHeader & 0x800) != 0,
      Inactive = (tileHeader & 0x40) != 0,
      InvisibleBlock = (tile.Header3 & 0x20) != 0,
      InvisibleWall = (tile.Header3 & 0x40) != 0,
      FullbrightBlock = (tile.Header3 & 0x80) != 0,
      FullbrightWall = (tileHeader & 0x8000) != 0
    };
  }
}

public sealed class PlayerSpawnPacketHandler(WorldSynchronizationObservation observation)
    : IPacketHandler<PlayerSpawnPacket> {
  private readonly WorldSynchronizationObservation _observation = observation
      ?? throw new ArgumentNullException(nameof(observation));

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      PlayerSpawnPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (packet.SpawnContext > 3) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "InvalidSpawnContext"));
    }
    byte playerSlot = context.Actor.PlayerSlot;
    _observation.RecordPlayerSpawn(playerSlot, packet.SpawnX, packet.SpawnY,
        packet.SpawnContext);
    var dispatch = new OutboundDispatch(new FinishedConnectingToServerPacket(),
        PacketDispatchKind.Single, [context.Connection], allowedStages: NetworkSessionStage.Active);
    return ValueTask.FromResult(new PacketHandlingResult(true, [dispatch],
        nextStage: NetworkSessionStage.Active));
  }
}

public sealed class PlayerControlsPacketHandler(PlayerControlsObservationStore observation,
    int worldWidth, int worldHeight) : IPacketHandler<PlayerControlsPacket> {
  private readonly PlayerControlsObservationStore _observation = observation
      ?? throw new ArgumentNullException(nameof(observation));

  public ValueTask<PacketHandlingResult> HandleAsync(NetworkSessionContext context,
      PlayerControlsPacket packet, CancellationToken cancellationToken) {
    cancellationToken.ThrowIfCancellationRequested();
    if (!float.IsFinite(packet.Position.X) || !float.IsFinite(packet.Position.Y)
        || packet.Position.X < 0 || packet.Position.Y < 0
        || packet.Position.X >= worldWidth * 16f || packet.Position.Y >= worldHeight * 16f
        || packet.Velocity is PacketVector2 velocity
            && (!float.IsFinite(velocity.X) || !float.IsFinite(velocity.Y)
                || Math.Abs(velocity.X) > 50 || Math.Abs(velocity.Y) > 50)) {
      return ValueTask.FromResult(new PacketHandlingResult(false,
          rejectionCode: "InvalidPlayerControls"));
    }

    _observation.Record(context.Actor.PlayerSlot, packet);
    return ValueTask.FromResult(new PacketHandlingResult(true));
  }
}

public sealed class PlayerControlsObservationStore {
  private readonly object _gate = new();
  private readonly Dictionary<byte, PlayerControlsObservation> _players = new();

  public IReadOnlyList<PlayerControlsObservation> Snapshot() {
    lock (_gate) {
      return Array.AsReadOnly(_players.Values.OrderBy(item => item.PlayerSlot).ToArray());
    }
  }

  public void Record(byte playerSlot, PlayerControlsPacket packet) {
    lock (_gate) {
      if (!_players.TryGetValue(playerSlot, out PlayerControlsObservation? previous)) {
        previous = new PlayerControlsObservation(playerSlot, 0, false, false, [],
            packet.Position, packet.Position, 0);
      }
      var sample = new PlayerControlsSample(packet.ControlFlags, packet.Position,
          packet.Velocity);
      var samples = previous.Samples.Append(sample).ToArray();
      if (samples.Length > 128) {
        samples = samples[^128..];
      }
      float dx = packet.Position.X - previous.LastPosition.X;
      float dy = packet.Position.Y - previous.LastPosition.Y;
      _players[playerSlot] = new PlayerControlsObservation(playerSlot,
          previous.PacketCount + 1,
          previous.SawRight || (packet.ControlFlags & 0x08) != 0,
          previous.SawJump || (packet.ControlFlags & 0x10) != 0,
          Array.AsReadOnly(samples), previous.FirstPosition, packet.Position,
          previous.TraveledDistancePixels + Math.Sqrt(dx * dx + dy * dy));
    }
  }

  public bool TryGetLastPosition(byte playerSlot, out PacketVector2 position) {
    lock (_gate) {
      if (_players.TryGetValue(playerSlot, out PlayerControlsObservation? observation)) {
        position = observation.LastPosition;
        return true;
      }
    }
    position = default;
    return false;
  }
}

public sealed record PlayerControlsObservation(byte PlayerSlot, int PacketCount,
    bool SawRight, bool SawJump, IReadOnlyList<PlayerControlsSample> Samples,
    PacketVector2 FirstPosition, PacketVector2 LastPosition, double TraveledDistancePixels);

public sealed record PlayerControlsSample(byte ControlFlags, PacketVector2 Position,
    PacketVector2? Velocity);

public sealed class WorldSynchronizationObservation {
  private readonly object _gate = new();
  private readonly List<WorldSectionObservation> _sections = new();
  private readonly List<PlayerSpawnObservation> _playerSpawns = new();

  public IReadOnlyList<WorldSectionObservation> Sections {
    get { lock (_gate) { return Array.AsReadOnly(_sections.ToArray()); } }
  }

  public IReadOnlyList<PlayerSpawnObservation> PlayerSpawns {
    get { lock (_gate) { return Array.AsReadOnly(_playerSpawns.ToArray()); } }
  }

  internal void RecordSection(int sectionX, int sectionY, string reason) {
    lock (_gate) {
      _sections.Add(new WorldSectionObservation(sectionX, sectionY, reason));
    }
  }

  internal void RecordPlayerSpawn(byte playerSlot, short spawnX, short spawnY,
      byte spawnContext) {
    lock (_gate) {
      _playerSpawns.Add(new PlayerSpawnObservation(playerSlot, spawnX, spawnY, spawnContext));
    }
  }
}

public sealed record WorldSectionObservation(int SectionX, int SectionY, string Reason);

public sealed record PlayerSpawnObservation(byte PlayerSlot, short SpawnX, short SpawnY,
    byte SpawnContext);

public sealed class TileBreakObservationStore {
  private readonly object _gate = new();
  private readonly List<TileBreakObservation> _breaks = new();

  public IReadOnlyList<TileBreakObservation> Snapshot() {
    lock (_gate) {
      return Array.AsReadOnly(_breaks.ToArray());
    }
  }

  internal void Record(byte playerSlot, short x, short y, ushort originalType,
      bool activeAfter) {
    lock (_gate) {
      _breaks.Add(new TileBreakObservation(playerSlot, x, y, originalType, activeAfter));
    }
  }
}

public sealed record TileBreakObservation(byte PlayerSlot, short X, short Y,
    ushort OriginalType, bool ActiveAfter);

public sealed class TileFrameRepairDispatchResult {
  public bool Accepted { get; }
  public string? RejectionCode { get; }
  public IReadOnlyList<OutboundDispatch> Outbound { get; }

  private TileFrameRepairDispatchResult(bool accepted,
      IEnumerable<OutboundDispatch>? outbound, string? rejectionCode) {
    Accepted = accepted;
    RejectionCode = rejectionCode;
    Outbound = Array.AsReadOnly(outbound?.ToArray() ?? []);
  }

  public static TileFrameRepairDispatchResult Succeeded(
      IEnumerable<OutboundDispatch> outbound) => new(true, outbound, null);

  public static TileFrameRepairDispatchResult Rejected(string rejectionCode) =>
      new(false, null, rejectionCode);
}

public sealed class TileFrameRepairPublicationResult {
  public bool Accepted { get; }
  public string? RejectionCode { get; }
  public int PublishedDispatchCount { get; }

  private TileFrameRepairPublicationResult(bool accepted, string? rejectionCode,
      int publishedDispatchCount) {
    Accepted = accepted;
    RejectionCode = rejectionCode;
    PublishedDispatchCount = publishedDispatchCount;
  }

  public static TileFrameRepairPublicationResult Succeeded(int publishedDispatchCount) =>
      new(true, null, publishedDispatchCount);

  public static TileFrameRepairPublicationResult Rejected(string rejectionCode,
      int publishedDispatchCount) => new(false, rejectionCode, publishedDispatchCount);
}
