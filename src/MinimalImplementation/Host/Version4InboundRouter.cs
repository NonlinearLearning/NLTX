using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.QuickStack;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.Modules;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Host;

/// <summary>Complete callback set required to bind a Version4 router to a connection host.</summary>
public sealed record Version4InboundCallbacks(
  Func<SessionEpoch> AllocateEpoch,
  Func<InboundRawFrame, InboundPacketEnvelope> CreateEnvelope,
  Func<InboundVersion4Packet, CancellationToken, ValueTask> OnPacket,
  Func<ConnectionClosed, ValueTask> OnClosed,
  Func<Exception, ValueTask> OnConnectionFailure,
  Func<SessionEpoch, ValueTask> OnConnected);

/// <summary>
/// Routes the transport-owned Version4 boundary into the single server commit loop.
///
/// The router owns the complete transport, admission, and commit-loop callback set. Packet
/// owners that are outside the migrated domain scope are rejected explicitly; a bounded rejection
/// is still a dispatcher decision and is never reported as a successful domain mutation.
/// </summary>
public sealed partial class Version4InboundRouter
{
  private const int Version4MessageCount = 162;
  private const byte _tileEntityPlacementMessageId = 87;

  private readonly ServerComposition _composition;
  private readonly ConcurrentDictionary<SessionEpoch, SessionStage> _stages = [];
  private readonly WorldDataPacketProducer _worldDataProducer;
  private readonly Func<WorldSnapshot, WorldDataPacketSupplementSnapshot>?
    _captureWorldDataSupplement;
  private readonly WorldSectionRequestCoordinator? _worldSectionRequests;
  private readonly object _connectionGate = new();
  private ServerConnectionHost? _connectionHost;
  private long _nextEpoch;
  private string? _lastPolicyRejection;

  public Version4InboundRouter(
    ServerComposition composition,
    Func<WorldSnapshot, WorldDataPacketSupplementSnapshot>? captureWorldDataSupplement = null,
    Func<WorldSectionSnapshot, WorldSectionPacketAttachments>? captureSectionAttachments = null)
  {
    ArgumentNullException.ThrowIfNull(composition);
    _composition = composition;
    _worldDataProducer = new WorldDataPacketProducer(composition.Version4WireProfile);
    _captureWorldDataSupplement = captureWorldDataSupplement;
    if (captureSectionAttachments is not null)
    {
      _worldSectionRequests = new WorldSectionRequestCoordinator(
        composition.WorldOwner,
        new WorldSectionPacketEncoder(composition.Version4WorldFacts, composition.Version4WireProfile),
        new WorldSectionDeliveryLedger(),
        captureSectionAttachments);
    }
  }

  /// <summary>
  /// Indicates that the router has a complete transport/admission boundary. Typed owners may
  /// still return a domain-level NotReady result when their world or attachment facts are not
  /// available; that is a request outcome rather than a missing dispatcher callback.
  /// </summary>
  public bool IsComplete =>
    _connectionHost is not null
    && _captureWorldDataSupplement is not null
    && _worldSectionRequests is not null
    && _composition.ModuleDispatcher.IsComplete
    && _composition.Version4WireProfile.DecodeBindingCount >= Version4MessageCount
    && _composition.Version4WireProfile.EncodeBindingCount >= Version4MessageCount;

  /// <summary>Captures the complete transport callback set, including session admission.</summary>
  public Version4InboundCallbacks CreateCallbacks()
  {
    return new Version4InboundCallbacks(
      AllocateEpoch,
      CreateEnvelope,
      OnPacket,
      OnClosed,
      OnConnectionFailure,
      OnConnected);
  }

  /// <summary>Allocates a process-local, non-zero epoch for one accepted socket.</summary>
  public SessionEpoch AllocateEpoch()
  {
    long value = Interlocked.Increment(ref _nextEpoch);
    if (value <= 0)
    {
      throw new InvalidOperationException("The session epoch range is exhausted.");
    }

    return SessionEpoch.Create((ulong)value);
  }

  /// <summary>Attaches the transport sink after the composition has created its host.</summary>
  public void AttachConnectionHost(ServerConnectionHost connectionHost)
  {
    ArgumentNullException.ThrowIfNull(connectionHost);
    lock (_connectionGate)
    {
      if (_connectionHost is not null)
      {
        throw new InvalidOperationException("The inbound router already has a connection host.");
      }

      _connectionHost = connectionHost;
    }
  }

  /// <summary>
  /// Creates an immutable envelope from transport-owned facts. Admission state is read only from
  /// the concurrent stage snapshot because this method runs on a receive worker thread.
  /// </summary>
  public InboundPacketEnvelope CreateEnvelope(InboundRawFrame frame)
  {
    ArgumentNullException.ThrowIfNull(frame);
    SessionStage stage = _stages.TryGetValue(frame.Epoch, out SessionStage capturedStage)
      ? capturedStage
      : SessionStage.Closed;
    return new InboundPacketEnvelope(
      frame.Epoch,
      frame.Sequence,
      frame.ReceivedAt,
      frame.Frame.MessageId,
      frame.Frame.ModuleId,
      frame.Frame.FrameBytes,
      stage.ToString(),
      _composition.IdentityRegistry.CurrentWorldScope,
      entityGeneration: null,
      expectedRevision: null);
  }

  /// <summary>Opens the session on the domain commit boundary before its first frame is read.</summary>
  public async ValueTask OnConnected(SessionEpoch epoch)
  {
    OwnerCommitResult? result = null;
    await _composition.CommitLoop.SubmitAsync(
      () =>
      {
        result = _composition.SessionAdmission.OpenConnection(epoch);
        RefreshStage(epoch);
      },
      CancellationToken.None).ConfigureAwait(false);

    if (result is null || result.Status != OwnerCommitStatus.Applied)
    {
      throw new InvalidOperationException(
        result?.Detail ?? "The accepted connection could not open a session.");
    }
  }

  /// <summary>Applies the wire profile's bounded handling policy and dispatches typed owners.</summary>
  public ValueTask OnPacket(
    InboundVersion4Packet packet,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.DecodedPacket is HelloPacket hello &&
        _composition.SessionAdmissionPolicy.IgnoreProtocolVersion &&
        !string.Equals(
          hello.Version,
          _composition.SessionAdmissionPolicy.ProtocolVersion,
          StringComparison.Ordinal))
    {
      ServerLog.Info(
        "server.session.protocol_version_ignored",
        $"epoch={packet.Envelope.Epoch.Value} " +
        $"received={ServerLog.Quote(hello.Version)} " +
        $"expected={ServerLog.Quote(_composition.SessionAdmissionPolicy.ProtocolVersion)}");
    }

    _composition.ModuleDispatcher.EvictExpiredReplayResults(packet.Envelope.ReceivedAt);
    return packet.Handling == Version4PacketHandling.Typed
      ? DispatchTypedAsync(packet, cancellationToken)
      : ConsumeByWirePolicy(packet);
  }

  /// <summary>Closes admission and all epoch-owned state on the commit boundary.</summary>
  public async ValueTask OnClosed(ConnectionClosed closed)
  {
    ArgumentNullException.ThrowIfNull(closed);
    SessionCloseResult? result = null;
    var intents = new List<OutboundPacketIntent>();
    ServerConnectionHost.OutboundTurn? outputTurn = null;
    await _composition.CommitLoop.SubmitAsync(
      () =>
      {
        IReadOnlyList<SessionEpoch> recipients =
          _composition.SessionAdmission.CaptureActiveRecipients(closed.Epoch);
        CommitVersion closeVersion = _composition.SessionAdmission.AllocateCommitVersion();
        IReadOnlyList<OutboundPacketIntent> lifecycleIntents =
          Array.Empty<OutboundPacketIntent>();
        try {
          result = _composition.SessionCloseCoordinator.Close(
            closed,
            recipients,
            closeVersion,
            context => ReleaseDependentSessionState(context, recipients, intents),
            projections => CaptureSessionOutputIntents(
              SessionAdmissionCoordinator.PlayerActiveMessageId, projections,
              out lifecycleIntents));
          if (result.IsCompleted) {
            intents.AddRange(lifecycleIntents);
          }
        } finally {
          RefreshStage(closed.Epoch);
        }

        outputTurn = ReserveOutboundTurn(null, intents);
      }).ConfigureAwait(false);

    using ServerConnectionHost.OutboundTurn? outputOrder = outputTurn;
    if (outputOrder is not null) {
      await outputOrder.Ready.ConfigureAwait(false);
    }

    if (result is null)
    {
      throw new InvalidOperationException("The close transition did not produce a result.");
    }

    // A rejected cleanup may already have committed tracker changes or projectile removals.
    // Submit only this attempt's outputs before reporting that the binding remains Closing.
    string? deliveryFailure = await SendCloseProjectionsAsync(intents).ConfigureAwait(false);
    if (deliveryFailure is not null) {
      Volatile.Write(ref _lastPolicyRejection, deliveryFailure);
    }

    if (!result.IsCompleted) {
      throw new InvalidDataException(
        result.Result.Detail ?? "Session cleanup did not complete; the epoch remains Closing.");
    }

    if (deliveryFailure is not null) {
      throw new InvalidDataException(deliveryFailure);
    }
  }

  /// <summary>Connection failures are transport diagnostics; close cleanup owns domain effects.</summary>
  public ValueTask OnConnectionFailure(Exception exception)
  {
    ArgumentNullException.ThrowIfNull(exception);
    return ValueTask.CompletedTask;
  }

  public string? LastPolicyRejection => Volatile.Read(ref _lastPolicyRejection);

  private OwnerCommitResult ReleaseDependentSessionState(
      SessionCloseContext context,
      IReadOnlyList<SessionEpoch> recipients,
      List<OutboundPacketIntent> intents) {
    SessionEpoch epoch = context.Connection.Epoch;
    _pendingItemDrops.Remove(epoch);
    OwnerCommitResult inventoryCleanup = _composition.WorldItemLifecycle.ReleaseSession(epoch);
    if (inventoryCleanup.Status != OwnerCommitStatus.Applied) {
      return inventoryCleanup;
    }

    OwnerCommitResult combatCleanup = ReleaseCombatSessionState(context, recipients, intents);
    if (combatCleanup.Status != OwnerCommitStatus.Applied) {
      return combatCleanup;
    }

    _composition.WorldEventCoordinator.RetireEpoch(epoch);
    _composition.ModuleDispatcher.RetireEpoch(epoch);
    _worldSectionRequests?.RetireSession(epoch);
    return inventoryCleanup;
  }

  private async ValueTask DispatchTypedAsync(
    InboundVersion4Packet inbound,
    CancellationToken cancellationToken)
  {
    var intents = new List<OutboundPacketIntent>();
    var deliverySettlements = new List<IOutboundDeliverySettlement>();
    IReadOnlyList<OutboundPacketIntent> admissionIntents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> worldDataIntents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> appearanceIntents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> controlsIntents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> playerStatusIntents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> lifeIntents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> loadoutIntents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> spawnIntents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> teamChangeIntents = Array.Empty<OutboundPacketIntent>();
    bool closeAfterResponse = false;
    string? rejection = null;
    WorldSectionSendBatch? pendingSectionRequest = null;
    WorldSectionSendBatch? pendingMovementSections = null;
    PendingJoinSynchronization? pendingJoin = null;
    PendingTeamSpawn? pendingTeamSpawn = null;
    ServerConnectionHost.OutboundTurn? outputTurn = null;
    Guid transferId = inbound.DecodedPacket is SpawnTileDataPacket ? Guid.NewGuid() : Guid.Empty;

    await _composition.CommitLoop.SubmitAsync(
      () =>
      {
        switch (inbound.DecodedPacket)
        {
          case HelloPacket hello:
          {
            SessionAdmissionPreparation preparation =
              _composition.SessionPacketHandlers.HandleHello(
                inbound.Envelope,
                hello,
                isBanned: false);
            SessionTransitionCommitResult committed =
              _composition.SessionPacketHandlers.CommitAdmission(
                preparation,
                _composition.Version4WireProfile);
            if (committed.Result.Status == OwnerCommitStatus.Applied)
            {
              if (committed.Response is not null)
              {
                OwnerCommitResult inventory = EnsureAdmissionInventory(
                  inbound.Envelope.Epoch,
                  committed.Response);
                if (inventory.Status == OwnerCommitStatus.Applied)
                {
                  admissionIntents = [committed.Response];
                }
                else
                {
                  rejection = inventory.Detail ?? "The initial player inventory could not be created.";
                  closeAfterResponse = true;
                }
              }

              closeAfterResponse |= committed.CloseAfterResponse;
            }
            else
            {
              rejection = committed.Result.Detail ?? "Hello admission was rejected.";
            }

            break;
          }
          case SendPasswordPacket password:
          {
            SessionAdmissionPreparation preparation =
              _composition.SessionPacketHandlers.HandlePassword(
                inbound.Envelope,
                password);
            SessionTransitionCommitResult committed =
              _composition.SessionPacketHandlers.CommitAdmission(
                preparation,
                _composition.Version4WireProfile);
            if (committed.Result.Status == OwnerCommitStatus.Applied)
            {
              if (committed.Response is not null)
              {
                OwnerCommitResult inventory = EnsureAdmissionInventory(
                  inbound.Envelope.Epoch,
                  committed.Response);
                if (inventory.Status == OwnerCommitStatus.Applied)
                {
                  admissionIntents = [committed.Response];
                }
                else
                {
                  rejection = inventory.Detail ?? "The initial player inventory could not be created.";
                  closeAfterResponse = true;
                }
              }

              closeAfterResponse |= committed.CloseAfterResponse;
            }
            else
            {
              rejection = committed.Result.Detail ?? "Password admission was rejected.";
            }

            break;
          }
          case HostTokenPacket hostToken:
          {
            OwnerCommitResult committed = _composition.SessionPacketHandlers.HandleHostToken(
              inbound.Envelope,
              hostToken);
            if (committed.Status != OwnerCommitStatus.Applied)
            {
              rejection = committed.Detail ?? "Host-token authorization was rejected.";
            }

            break;
          }
          case PingPacket ping:
          {
            IReadOnlyList<OutboundPacketIntent> pingIntents = Array.Empty<OutboundPacketIntent>();
            SessionPingResult result = _composition.SessionPacketHandlers.HandlePing(
              inbound.Envelope,
              ping,
              projections => CaptureSessionOutputIntents(
                SessionAdmissionCoordinator.PingMessageId,
                projections,
                out pingIntents));
            if (result.Result.Status == OwnerCommitStatus.Applied)
            {
              intents.AddRange(pingIntents);
            }
            else
            {
              rejection = result.Result.Detail ?? "Ping was rejected.";
            }

            break;
          }
          case SpawnTileDataPacket spawnTileData:
          {
            rejection = HandleInitialJoin(
              inbound.Envelope, spawnTileData, transferId, out pendingJoin);
            closeAfterResponse = rejection is not null &&
              _composition.SessionAdmission.TryGetStage(
                inbound.Envelope.Epoch, out SessionStage joinStage) &&
              joinStage == SessionStage.Synchronizing;

            break;
          }
          case SyncPlayerPacket appearance:
          {
            WorldHeaderCapture world;
            try {
              world = _composition.WorldOwner.CaptureWorldHeader();
            } catch (InvalidOperationException exception) {
              rejection = $"Packet 4 is NotReady: {exception.Message}";
              break;
            }

            rejection = HandleAppearanceUpload(
              inbound.Envelope, appearance, world.Header.Facts.GameMode == 3,
              out appearanceIntents, out closeAfterResponse);
            break;
          }
          case Unknown68Packet clientUuid:
            rejection = ConsumeClientUuid(inbound.Envelope, clientUuid);
            break;
          case SyncEquipmentPacket equipment:
            rejection = HandleEquipmentUpload(inbound.Envelope, equipment);
            break;
          case TileManipulationPacket tileManipulation:
            rejection = HandleTileManipulation(inbound.Envelope, tileManipulation, intents);
            break;
          case DamageNPCPacket npcDamage:
            rejection = HandleNpcDamage(inbound.Envelope, npcDamage, intents, deliverySettlements);
            break;
          case SyncItemPacket item:
            rejection = HandleWorldItemUpload(inbound.Envelope, item);
            break;
          case ReleaseItemOwnershipPacket releaseItem:
            rejection = HandleWorldItemRelease(inbound.Envelope, releaseItem);
            break;
          case SyncItemDespawnPacket retireItem:
            rejection = HandleWorldItemRetirement(inbound.Envelope, retireItem);
            break;
          case PlayerControlsPacket controls:
            rejection = HandleControlsUpload(inbound.Envelope, controls, out controlsIntents);
            if (rejection is null) {
              rejection = CaptureMovementSections(inbound.Envelope.Epoch,
                out pendingMovementSections);
              closeAfterResponse = rejection is not null;
            }
            break;
          case PlayerLifeManaPacket lifeMana:
            rejection = HandleLifeUpload(inbound.Envelope, lifeMana, out lifeIntents);
            break;
          case TogglePVPPacket togglePvp:
            rejection = HandlePvpUpload(inbound.Envelope, togglePvp, out playerStatusIntents);
            break;
          case PlayerHealPacket heal:
            rejection = HandleHealEffect(inbound.Envelope, heal, out playerStatusIntents);
            break;
          case SyncPlayerZonePacket zones:
            rejection = HandleZoneUpload(inbound.Envelope, zones, out playerStatusIntents);
            break;
          case SyncTilePickingPacket tilePicking:
            rejection = HandleTilePicking(inbound.Envelope, tilePicking, out playerStatusIntents);
            break;
          case Unknown42Packet mana:
            AddMutation(
              _composition.PlayerStatePacketHandlers.HandleMana(
                inbound.Envelope,
                mana),
              intents,
              ref closeAfterResponse,
              ref rejection);
            break;
          case TeamChangePacket teamChange:
            rejection = HandleTeamChange(inbound.Envelope, teamChange, out teamChangeIntents);
            break;
          case PlayerStealthPacket stealth:
            rejection = HandleStealthUpload(inbound.Envelope, stealth, out playerStatusIntents);
            break;
          case MinionRestTargetUpdatePacket restTarget:
            rejection = HandleMinionRestTargetUpload(
              inbound.Envelope, restTarget, out playerStatusIntents);
            break;
          case MinionAttackTargetUpdatePacket attackTarget:
            rejection = HandleMinionAttackTargetUpload(
              inbound.Envelope, attackTarget, out playerStatusIntents);
            break;
          case Unknown66Packet healToPlayer:
            rejection = HandleHealToPlayer(inbound.Envelope, healToPlayer, out playerStatusIntents);
            break;
          case PlayerSpawnPacket spawn:
          {
            if (_composition.SessionAdmission.TryGetStage(
                  inbound.Envelope.Epoch, out SessionStage spawnStage) &&
                spawnStage == SessionStage.Synchronizing &&
                _worldSectionRequests?.HasCompletedInitialSync(inbound.Envelope.Epoch) != true) {
              rejection = "Packet 12 requires completed initial synchronization delivery.";
              break;
            }

            rejection = HandleSpawn(inbound.Envelope, spawn, out spawnIntents);
            break;
          }
          case QuickStackChestsRequestPacket quickStack:
          {
            P04InventoryPacketHandlers? inventoryHandlers = _composition.P04InventoryHandlers;
            if (inventoryHandlers is null)
            {
              rejection =
                "Packet 85 is NotReady: the P04 inventory handler bundle is not registered.";
              break;
            }

            QuickStackPacketDispatchResult dispatched = inventoryHandlers.QuickStackPacket85.Handle(
              inbound.Envelope,
              quickStack);
            if (dispatched.IsApplied)
            {
              intents.AddRange(dispatched.OutboundIntents);
            }
            else
            {
              rejection = dispatched.Commit.Detail ?? "Packet 85 QuickStack was rejected.";
            }

            break;
          }
          case SpectatePlayerPacket spectate:
            rejection = ApplySpectate(inbound.Envelope, spectate, intents);
            break;
          case RequestWorldDataPacket:
            rejection = HandleWorldDataRequest(inbound.Envelope, out worldDataIntents);
            break;
          case RequestSectionPacket requestSection:
            rejection = HandleSectionRequest(
              inbound.Envelope,
              requestSection,
              out pendingSectionRequest);
            break;
          case OpenSignRequestPacket signRequest:
            rejection = HandleSignOpenRequest(
              inbound.Envelope,
              signRequest,
              intents);
            break;
          case OpenSignResponsePacket signUpdate:
            rejection = HandleSignUpdate(
              inbound.Envelope,
              signUpdate,
              intents);
            break;
          case TileEntityPlacementPacket tileEntityPlacement:
            rejection = HandleTileEntityPlacement(
              inbound.Envelope,
              tileEntityPlacement,
              intents);
            break;
          case ExtraSpawnSectionLoadedPacket extraSpawnLoaded:
          {
            OwnerCommitResult consumed =
              _composition.PlayerTeamChangeCoordinator.HandleExtraSpawnSectionLoaded(
                inbound.Envelope, extraSpawnLoaded);
            if (consumed.Status != OwnerCommitStatus.Applied) {
              rejection = consumed.Detail ?? "Extra-spawn section acknowledgement was rejected.";
            }

            break;
          }
          case PlayerBuffsPacket buffs:
            rejection = HandleBuffUpload(inbound.Envelope, buffs, out playerStatusIntents);
            break;
          case UpdatePlayerLuckFactorsPacket luck:
            AddMutation(
              _composition.PlayerStatePacketHandlers.HandleLuck(
                inbound.Envelope,
                luck,
                player => new PlayerLuckCalculationContext(
                  _composition.LanternNightState.CaptureSnapshot().LanternsUp,
                  player.Buffs.HasBuff(Version4PlayerBuffCatalog.StinkyBuffType))),
              intents,
              ref closeAfterResponse,
              ref rejection);
            break;
          case SyncLoadoutPacket loadout:
            rejection = HandleLoadout(inbound.Envelope, loadout, out loadoutIntents);
            break;
          case TeamChangeFromUIPacket teamChangeFromUi:
            rejection = HandleTeamChangeFromUi(
              inbound.Envelope, teamChangeFromUi, out teamChangeIntents,
              out pendingTeamSpawn, out bool closeTeamChange);
            closeAfterResponse |= closeTeamChange;
            break;
          case PlayerDeathV2Packet death:
            rejection = HandleDeathUpload(
              inbound.Envelope, death, out playerStatusIntents);
            break;
          case NetModulesPacket modulePacket:
          {
            ModuleDispatchResult dispatched = DispatchModule(inbound, modulePacket);
            if (dispatched.Disposition == ModuleDispatchDisposition.Handled)
            {
              intents.AddRange(dispatched.OutboundIntents);
              if (dispatched.DeliverySettlement is not null)
              {
                deliverySettlements.Add(dispatched.DeliverySettlement);
              }
            }
            else
            {
              rejection = dispatched.Detail ?? "Packet 82 module dispatch was rejected.";
            }

            break;
          }
          default:
            rejection =
              $"Typed Version4 packet {inbound.Envelope.MessageId} has no host route yet.";
            break;
        }

        RefreshStage(inbound.Envelope.Epoch);
        // Module settlements contain the same intents as their dispatch result. Deferred
        // section output targets the requesting epoch, including an empty section batch.
        SessionEpoch? sectionTarget =
          pendingJoin is not null || pendingSectionRequest is not null ||
            pendingMovementSections is not null || pendingTeamSpawn is not null
            ? inbound.Envelope.Epoch
            : null;
        outputTurn = ReserveOutboundTurn(
          sectionTarget, intents, admissionIntents, worldDataIntents, appearanceIntents,
          controlsIntents, playerStatusIntents, lifeIntents, loadoutIntents, spawnIntents,
          teamChangeIntents, pendingJoin?.BeforeSections ?? [], pendingJoin?.AfterSections ?? [],
          pendingJoin is null ? [] : [pendingJoin.InitialSpawn],
          pendingTeamSpawn is null ? [] : [pendingTeamSpawn.Acknowledgement]);
      },
      cancellationToken).ConfigureAwait(false);

    using ServerConnectionHost.OutboundTurn? outputOrder = outputTurn;
    if (outputOrder is not null) {
      // Releasing a cancelled waiter before its predecessors would let successors overtake.
      await outputOrder.Ready.ConfigureAwait(false);
      if (cancellationToken.IsCancellationRequested) {
        // The commit may have opened transfers before cancellation while waiting to send.
        if (pendingJoin is not null) {
          await RetireSectionTransferAsync(pendingJoin.Sections).ConfigureAwait(false);
        }

        if (pendingSectionRequest is not null) {
          await RetireSectionTransferAsync(pendingSectionRequest).ConfigureAwait(false);
        }

        if (pendingMovementSections is not null) {
          await RetireSectionTransferAsync(pendingMovementSections).ConfigureAwait(false);
        }

        if (pendingTeamSpawn is not null) {
          await RetireSectionTransferAsync(pendingTeamSpawn.Sections).ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
      }
    }

    if (admissionIntents.Count > 0) {
      string? deliveryFailure = await SendCommittedSessionOutputAsync(
        inbound.Envelope.MessageId, admissionIntents, cancellationToken).ConfigureAwait(false);
      if (deliveryFailure is not null) {
        rejection = deliveryFailure;
        closeAfterResponse = true;
      }
    }

    var settledIntents = new HashSet<OutboundPacketIntent>(ReferenceEqualityComparer.Instance);
    foreach (IOutboundDeliverySettlement settlement in deliverySettlements)
    {
      foreach (OutboundPacketIntent intent in settlement.Intents)
      {
        settledIntents.Add(intent);
      }
    }

    foreach (OutboundPacketIntent intent in intents)
    {
      if (!settledIntents.Contains(intent))
      {
        await SendAsync(intent, cancellationToken).ConfigureAwait(false);
      }
    }

    if (worldDataIntents.Count > 0) {
      string? deliveryFailure = await SendCommittedSessionOutputAsync(
        SessionAdmissionCoordinator.RequestWorldDataMessageId, worldDataIntents, cancellationToken)
        .ConfigureAwait(false);
      if (deliveryFailure is not null) {
        rejection = deliveryFailure;
        closeAfterResponse = true;
      }
    }

    if (appearanceIntents.Count > 0) {
      rejection = await SendCommittedSessionOutputAsync(
        SessionAdmissionCoordinator.SyncPlayerMessageId, appearanceIntents, cancellationToken)
        .ConfigureAwait(false) ?? rejection;
    }

    if (lifeIntents.Count > 0) {
      rejection = await SendCommittedSessionOutputAsync(
        SessionAdmissionCoordinator.PlayerLifeManaMessageId, lifeIntents, cancellationToken)
        .ConfigureAwait(false) ?? rejection;
    }

    if (controlsIntents.Count > 0) {
      rejection = await SendCommittedSessionOutputAsync(
        SessionAdmissionCoordinator.PlayerControlsMessageId, controlsIntents, cancellationToken)
        .ConfigureAwait(false) ?? rejection;
    }

    if (playerStatusIntents.Count > 0) {
      rejection = await SendCommittedSessionOutputAsync(
        inbound.Envelope.MessageId, playerStatusIntents, cancellationToken)
        .ConfigureAwait(false) ?? rejection;
    }

    if (loadoutIntents.Count > 0) {
      rejection = await SendCommittedSessionOutputAsync(
        SessionAdmissionCoordinator.SyncLoadoutMessageId, loadoutIntents, cancellationToken)
        .ConfigureAwait(false) ?? rejection;
    }

    if (spawnIntents.Count > 0) {
      rejection = await SendCommittedSessionOutputAsync(
        SessionAdmissionCoordinator.PlayerSpawnMessageId, spawnIntents, cancellationToken)
        .ConfigureAwait(false) ?? rejection;
    }

    if (teamChangeIntents.Count > 0) {
      var failedTargets = new HashSet<SessionEpoch>();
      rejection = await SendCommittedSessionOutputAsync(
        inbound.Envelope.MessageId, teamChangeIntents, cancellationToken, failedTargets)
        .ConfigureAwait(false) ?? rejection;
      if (pendingTeamSpawn is not null && failedTargets.Contains(inbound.Envelope.Epoch)) {
        // Do not send sections/158 after the requesting player's team notification failed.
        await RetireSectionTransferAsync(pendingTeamSpawn.Sections).ConfigureAwait(false);
        pendingTeamSpawn = null;
        closeAfterResponse = true;
      }
    }

    var settlementWorks = new List<OutboundSettlementWork>(deliverySettlements.Count);
    foreach (IOutboundDeliverySettlement settlement in deliverySettlements)
    {
      var deliveries = new List<OutboundIntentDelivery>(settlement.Intents.Count);
      foreach (OutboundPacketIntent intent in settlement.Intents)
      {
        IReadOnlyList<SendReceipt> receipts = await SendWithReceiptsAsync(
          intent,
          cancellationToken).ConfigureAwait(false);
        if (receipts.Count == 0)
        {
          receipts = [CreateNotSubmittedReceipt(intent)];
        }

        deliveries.Add(new OutboundIntentDelivery(intent, receipts));
      }

      settlementWorks.Add(new OutboundSettlementWork(settlement, deliveries));
    }

    if (settlementWorks.Count > 0)
    {
      var settlementResults = new List<OutboundDeliverySettlementResult>(settlementWorks.Count);
      // Returned transport receipts must reach their owner even if the connection cancels now.
      await _composition.CommitLoop.SubmitAsync(
        () =>
        {
          foreach (OutboundSettlementWork work in settlementWorks)
          {
            settlementResults.Add(work.Settlement.Settle(work.Deliveries));
          }
        }).ConfigureAwait(false);

      foreach (OutboundDeliverySettlementResult result in settlementResults)
      {
        if (!result.IsSettled)
        {
          rejection = result.Detail ??
            $"Outbound delivery settlement for {result.OwnerKey} was not completed.";
        }
      }

      cancellationToken.ThrowIfCancellationRequested();
    }

    if (pendingJoin is not null &&
        !await SendJoinSynchronizationAsync(pendingJoin, cancellationToken).ConfigureAwait(false)) {
      rejection = "Packet 8 synchronization delivery did not complete.";
      closeAfterResponse = true;
    }

    if (pendingSectionRequest is not null &&
        !await SendSectionBatchAsync(
          pendingSectionRequest, SessionStage.Active, cancellationToken).ConfigureAwait(false)) {
      rejection = "Packet 159 section delivery did not complete.";
      closeAfterResponse = true;
    }

    if (pendingTeamSpawn is not null &&
        !await SendTeamSpawnAsync(pendingTeamSpawn, cancellationToken).ConfigureAwait(false)) {
      rejection = "Packet 157 team-spawn section delivery did not complete.";
      closeAfterResponse = true;
    }

    if (pendingMovementSections is not null &&
        !await SendMovementSectionsAsync(pendingMovementSections, cancellationToken)
          .ConfigureAwait(false)) {
      rejection = "Movement section delivery did not complete.";
      closeAfterResponse = true;
    }

    if (rejection is not null)
    {
      Volatile.Write(ref _lastPolicyRejection, rejection);
      ServerLog.Warning(
        "server.packet.rejected",
        $"epoch={inbound.Envelope.Epoch.Value} messageId={inbound.Envelope.MessageId} " +
        $"detail={ServerLog.Quote(rejection)}");
    }

    if (closeAfterResponse)
    {
      await CloseAsync(inbound.Envelope.Epoch).ConfigureAwait(false);
    }
  }

  private string? HandleWorldDataRequest(
    InboundPacketEnvelope envelope,
    out IReadOnlyList<OutboundPacketIntent> intents)
  {
    intents = Array.Empty<OutboundPacketIntent>();
    if (_captureWorldDataSupplement is null || _worldSectionRequests is null)
    {
      return
        "Packet 6 is NotReady: the composition has no authoritative packet-7 or section owner.";
    }

    if (!_composition.WorldOwner.IsReady)
    {
      return "Packet 6 is NotReady: no published world is available.";
    }

    try
    {
      WorldSnapshot world = _composition.WorldOwner.CaptureSnapshot();
      WorldDataPacketSupplementSnapshot supplement = _captureWorldDataSupplement(world);
      WorldDataPacket packet = _worldDataProducer.CreatePacket(world, supplement);
      IReadOnlyList<OutboundPacketIntent> preparedIntents = Array.Empty<OutboundPacketIntent>();

      OwnerCommitResult PrepareWorldDataDelivery(
        IReadOnlyList<SessionPacketProjection> projections)
      {
        OwnerCommitResult encodedResult = CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.RequestWorldDataMessageId, projections,
          out IReadOnlyList<OutboundPacketIntent> encoded);
        if (encodedResult.Status != OwnerCommitStatus.Applied) {
          return encodedResult;
        }

        WorldSectionDeliveryResult begun = _worldSectionRequests.BeginWorld(envelope.Epoch, world);
        if (begun != WorldSectionDeliveryResult.Queued)
        {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
            $"Packet 6 is NotReady: section delivery initialization ended with {begun}.");
        }

        preparedIntents = encoded;
        return OwnerCommitResult.Applied(CommitVersion.Initial);
      }

      JoinStepResult joined = _composition.SessionPacketHandlers.HandleWorldDataRequest(
        envelope,
        packet,
        PrepareWorldDataDelivery);
      if (!joined.IsApplied)
      {
        return joined.Result.Detail ?? "Packet 6 world-data synchronization was rejected.";
      }

      intents = preparedIntents;
      return null;
    }
    catch (NotSupportedException exception)
    {
      return $"Packet 6 is NotReady: {exception.Message}";
    }
    catch (InvalidDataException exception)
    {
      return $"Packet 6 world facts were invalid: {exception.Message}";
    }
    catch (ArgumentException exception)
    {
      return $"Packet 6 world facts were rejected: {exception.Message}";
    }
    catch (InvalidOperationException exception)
    {
      return $"Packet 6 is NotReady: {exception.Message}";
    }
  }

  private ModuleDispatchResult DispatchModule(
    InboundVersion4Packet inbound,
    NetModulesPacket packet)
  {
    if (!_composition.SessionAdmission.TryGetBinding(
          inbound.Envelope.Epoch,
          out SessionBinding? senderBinding)
        || senderBinding is null
        || !_composition.Players.TryGetBoundPlayer(
          inbound.Envelope.Epoch,
          _composition.SessionAdmission,
        out PlayerEntity? sender)
        || sender is null)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.Forbidden,
        "Packet 82 requires an authenticated player binding.");
    }

    var recipients = new List<ModuleRecipient>();
    foreach (SessionBinding binding in _composition.SessionBindings.CaptureBindings())
    {
      if (binding.Epoch == inbound.Envelope.Epoch
        || !_composition.SessionAdmission.TryGetStage(binding.Epoch, out SessionStage stage)
        || stage != SessionStage.Active
        || !_composition.Players.TryGetForBinding(binding, out PlayerEntity? player)
        || player is null)
      {
        continue;
      }

      recipients.Add(new ModuleRecipient(
        binding.Epoch,
        binding.PlayerSlot,
        player.Team.Current));
    }

    ModuleSessionStage dispatchStage = _composition.SessionAdmission.TryGetStage(
      inbound.Envelope.Epoch,
      out SessionStage senderStage)
      ? ToModuleStage(senderStage)
      : ModuleSessionStage.Closing;
    var context = new ModuleDispatchContext(
      inbound.Envelope.Epoch,
      sender.Handle.Id,
      senderBinding.PlayerSlot,
      sender.Team.Current,
      dispatchStage,
      sender.CountsAsHostForGameplay,
      recipients);
    ushort logicalModuleId = Version4SteamModuleIdMap.ToLogicalModuleId(packet.ModuleId);
    if (logicalModuleId == ushort.MaxValue || logicalModuleId >= ModuleDispatcher.ModuleCount)
    {
      return ModuleDispatchResult.Rejected(
        ProtocolErrorCode.UnknownModule,
        $"Packet 82 module {packet.ModuleId} has no server logical module mapping.");
    }

    InboundPacketEnvelope logicalEnvelope = new(
      inbound.Envelope.Epoch,
      inbound.Envelope.Sequence,
      inbound.Envelope.ReceivedAt,
      inbound.Envelope.MessageId,
      logicalModuleId,
      inbound.Envelope.RawFrame,
      inbound.Envelope.Stage,
      inbound.Envelope.WorldScope,
      inbound.Envelope.EntityGeneration,
      inbound.Envelope.ExpectedRevision);
    return _composition.ModuleDispatcher.Dispatch(logicalEnvelope, context, packet.Data);
  }

  private static bool IsValidSpawnRequest(
    WorldGeometry geometry,
    SpawnTileDataPacket request)
  {
    bool defaultSpawn = request.X == -1 && request.Y == -1;
    bool selectedSpawn = request.X >= 10 && request.X <= geometry.Width - 10 &&
      request.Y >= 10 && request.Y <= geometry.Height - 10;
    return request.Team < 6 && (defaultSpawn || selectedSpawn);
  }

  private string? HandleSignOpenRequest(
    InboundPacketEnvelope envelope,
    OpenSignRequestPacket packet,
    List<OutboundPacketIntent> intents)
  {
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.OpenSignRequestMessageId,
      [SessionStage.Active],
      binding =>
      {
        if (!_composition.WorldOwner.IsReady)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "Packet 46 is not ready because no published world is available.");
        }

        WorldSnapshot world = _composition.WorldOwner.CaptureSnapshot();
        SignPacketDispatchResult dispatched = _composition.SignPacketHandler.HandleOpenRequest(
          envelope,
          binding.PlayerSlot,
          packet,
          world,
          world.Version);
        intents.AddRange(dispatched.OutboundIntents);
        return OwnerCommitResult.Applied(world.Version);
      });
    return result.Status == OwnerCommitStatus.Applied
      ? null
      : result.Detail ?? "Sign-open request was rejected.";
  }

  private string? HandleSignUpdate(
    InboundPacketEnvelope envelope,
    OpenSignResponsePacket packet,
    List<OutboundPacketIntent> intents)
  {
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.OpenSignResponseMessageId,
      [SessionStage.Active],
      binding =>
      {
        if (!_composition.WorldOwner.IsReady)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "Packet 47 is not ready because no published world is available.");
        }

        WorldSnapshot world = _composition.WorldOwner.CaptureSnapshot();
        SignPacketDispatchResult dispatched = _composition.SignPacketHandler.HandleUpdate(
          envelope,
          binding.PlayerSlot,
          packet,
          world,
          _composition.SessionAdmission.CaptureActiveRecipients(envelope.Epoch),
          world.Version);
        intents.AddRange(dispatched.OutboundIntents);
        return OwnerCommitResult.Applied(world.Version);
      });
    return result.Status == OwnerCommitStatus.Applied
      ? null
      : result.Detail ?? "Sign update was rejected.";
  }

  private string? HandleTileEntityPlacement(
    InboundPacketEnvelope envelope,
    TileEntityPlacementPacket packet,
    List<OutboundPacketIntent> intents)
  {
    OwnerCommitResult result = _composition.SessionAdmission.ExecuteAuthenticatedPlayerPacket(
      envelope,
      _tileEntityPlacementMessageId,
      [SessionStage.Active],
      binding =>
      {
        if (!_composition.WorldOwner.IsReady)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.NotReady,
            "Packet 87 requires a published world.");
        }

        WorldSnapshot world = _composition.WorldOwner.CaptureSnapshot();
        TileEntityPlacementDispatchResult dispatched =
          _composition.TileEntityPacketHandler.HandlePlacement(
            envelope,
            binding.PlayerSlot,
            packet,
            world,
            _composition.SessionAdmission.CaptureActiveSessionEpochs(),
            world.Version);
        if (dispatched.Status is TileEntityPlacementStatus.Created or
            TileEntityPlacementStatus.PylonTileRejected)
        {
          intents.AddRange(dispatched.OutboundIntents);
        }

        return OwnerCommitResult.Applied(world.Version);
      });
    return result.Status == OwnerCommitStatus.Applied
      ? null
      : result.Detail ?? "TileEntity placement was rejected.";
  }

  private string? ApplySpectate(
      InboundPacketEnvelope envelope,
      SpectatePlayerPacket packet,
      List<OutboundPacketIntent> intents) {
    PlayerSpectatingCommit committed = PlayerSpectatingCoordinator.HandleExplicitStop(
      _composition.Players, _composition.SessionAdmission, envelope, packet, projections => {
        return PrepareSpectatingStopOutput(projections, intents);
      });
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "Spectate request was rejected.";
  }

  private OwnerCommitResult PrepareSpectatingStopOutput(
      IReadOnlyList<SessionPacketProjection> projections,
      List<OutboundPacketIntent> intents) {
    try {
      IReadOnlyList<OutboundPacketIntent> encoded = _composition.SessionPacketHandlers.CreateIntents(
        projections, _composition.Version4WireProfile);
      intents.AddRange(encoded);
      return OwnerCommitResult.Applied(CommitVersion.Initial);
    } catch (Exception exception) when (exception is ArgumentException or
        InvalidDataException or InvalidOperationException or NotSupportedException) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
        $"Packet 150 output preparation failed: {exception.Message}");
    }
  }

  private void AddMutation(
    PlayerPacketMutationResult mutation,
    List<OutboundPacketIntent> intents,
    ref bool closeAfterResponse,
    ref string? rejection)
  {
    if (mutation.Result.Status != OwnerCommitStatus.Applied)
    {
      rejection = mutation.Result.Detail ??
        $"Packet {mutation.Result.ErrorCode?.ToString() ?? "unknown"} was rejected.";
    }

    OutboundPacketIntent? intent = _composition.SessionPacketHandlers.CreateIntent(
      mutation,
      _composition.Version4WireProfile);
    if (intent is not null)
    {
      intents.Add(intent);
    }

    closeAfterResponse |= mutation.CloseAfterResponse;
  }

  private ValueTask ConsumeByWirePolicy(InboundVersion4Packet packet)
  {
    if (packet.Handling is Version4PacketHandling.RejectStage
      or Version4PacketHandling.RejectDirection)
    {
      string detail =
        $"Packet {packet.Envelope.MessageId} rejected by wire policy: {packet.Reason}";
      Volatile.Write(ref _lastPolicyRejection, detail);
      ServerLog.Warning(
        "server.packet.wire_policy_rejected",
        $"epoch={packet.Envelope.Epoch.Value} messageId={packet.Envelope.MessageId} " +
        $"detail={ServerLog.Quote(packet.Reason)}");
    }

    return ValueTask.CompletedTask;
  }

  private async ValueTask SendAsync(
    OutboundPacketIntent intent,
    CancellationToken cancellationToken)
  {
    await SendWithReceiptsAsync(intent, cancellationToken).ConfigureAwait(false);
  }

  // Reserve only at the end of the original commit. The host shares these dependencies with
  // its periodic delivery pump; waiting and transport I/O stay outside the loop.
  private ServerConnectionHost.OutboundTurn? ReserveOutboundTurn(
      SessionEpoch? sectionTarget, params IReadOnlyList<OutboundPacketIntent>[] batches) {
    ServerConnectionHost? connectionHost;
    lock (_connectionGate) {
      connectionHost = _connectionHost;
    }

    if (connectionHost is null) {
      bool hasTargets = sectionTarget is not null || batches
        .SelectMany(batch => batch)
        .Any(intent => intent.TargetEpochs.Any(target => target != intent.IgnoreEpoch));
      if (!hasTargets) {
        return null;
      }

      throw new InvalidOperationException("The inbound router has no connection host.");
    }

    return connectionHost.ReserveOutboundTurn(_composition.CommitLoop, sectionTarget, batches);
  }

  private async ValueTask<IReadOnlyList<SendReceipt>> SendWithReceiptsAsync(
    OutboundPacketIntent intent,
    CancellationToken cancellationToken)
  {
    ServerConnectionHost connectionHost;
    lock (_connectionGate)
    {
      connectionHost = _connectionHost
        ?? throw new InvalidOperationException("The inbound router has no connection host.");
    }

    return await connectionHost.SendAsync(intent, cancellationToken).ConfigureAwait(false);
  }

  private static SendReceipt CreateNotSubmittedReceipt(OutboundPacketIntent intent)
  {
    OutboundDeliveryToken token = intent.DeliveryToken
      ?? throw new InvalidOperationException(
        "A target-specific outbound settlement intent requires a delivery token.");
    long sequence = checked((long)token.IntentIndex + 1);
    return new SendReceipt(
      TransportSendCertainty.NotSubmitted,
      token.Target,
      new PacketSequence(sequence),
      bytesSubmitted: 0,
      ProtocolErrorCode.StaleEpoch);
  }

  private async ValueTask CloseAsync(SessionEpoch epoch)
  {
    ServerConnectionHost connectionHost;
    lock (_connectionGate)
    {
      connectionHost = _connectionHost
        ?? throw new InvalidOperationException("The inbound router has no connection host.");
    }

    await connectionHost.CloseAsync(epoch).ConfigureAwait(false);
  }

  private void RefreshStage(SessionEpoch epoch)
  {
    if (_composition.SessionAdmission.TryGetStage(epoch, out SessionStage stage)
      && stage != SessionStage.Closed)
    {
      _stages[epoch] = stage;
      return;
    }

    _stages.TryRemove(epoch, out _);
  }

  private static ModuleSessionStage ToModuleStage(SessionStage stage)
  {
    return stage switch
    {
      SessionStage.Active => ModuleSessionStage.Active,
      SessionStage.Closing or SessionStage.Closed => ModuleSessionStage.Closing,
      SessionStage.AwaitHello or SessionStage.AwaitPassword => ModuleSessionStage.Connected,
      _ => ModuleSessionStage.Authenticated
    };
  }

  private sealed record OutboundSettlementWork(
    IOutboundDeliverySettlement Settlement,
    IReadOnlyList<OutboundIntentDelivery> Deliveries);
}
