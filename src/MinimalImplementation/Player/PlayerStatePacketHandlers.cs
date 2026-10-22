using System;
using System.Collections.Generic;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Thin packet adapters over the single PlayerOwner state authority.</summary>
public sealed class PlayerStatePacketHandlers
{
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly PlayerLifecycleCoordinator _lifecycle;

  public PlayerStatePacketHandlers(
    PlayerOwner players,
    SessionAdmissionCoordinator sessions,
    Func<SessionBinding, SyncProjectileTrackersPacket>? captureProjectileTrackers = null)
  {
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    _players = players;
    _sessions = sessions;
    _lifecycle = new PlayerLifecycleCoordinator(sessions, players, captureProjectileTrackers);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyLifeUpload"/>
  public PlayerPacketMutationResult HandleLifeMana(
    InboundPacketEnvelope envelope,
    PlayerLifeManaPacket packet,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _players.ApplyLifeUpload(envelope, packet, _sessions, prepareOutput);
  }

  public PlayerPacketMutationResult HandleMana(
    InboundPacketEnvelope envelope,
    Unknown42Packet packet)
  {
    return _players.ApplyManaUpload(envelope, packet, _sessions);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyAppearanceUpload"/>
  public PlayerPacketMutationResult HandleAppearance(
    InboundPacketEnvelope envelope,
    SyncPlayerPacket packet,
    int skinVariantCount,
    int maximumNameLength,
    bool isJourneyWorld,
    bool checkDuplicateName,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _players.ApplyAppearanceUpload(
      envelope,
      packet,
      _sessions,
      skinVariantCount,
      maximumNameLength,
      isJourneyWorld,
      checkDuplicateName,
      prepareOutput);
  }

  /// <summary>
  /// Prepares inventory and output before either owner writes. The output callback may encode
  /// and capture the projections; it must not send packets or mutate owner state.
  /// </summary>
  public PlayerLoadoutCommit HandleLoadout(
    InboundPacketEnvelope envelope,
    SyncLoadoutPacket packet,
    Func<SessionBinding, Player, SyncLoadoutPacket, IPlayerLoadoutTransaction> prepareP04Transaction,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(prepareP04Transaction);
    return HandleLoadoutCore(envelope, packet, prepareP04Transaction, null, prepareOutput);
  }

  /// <summary>
  /// Compatibility port. Production should use the prepared transaction overload.
  /// The callback must atomically validate and commit P04 state without changing PlayerOwner.
  /// </summary>
  public PlayerLoadoutCommit HandleLoadout(
    InboundPacketEnvelope envelope,
    SyncLoadoutPacket packet,
    Func<SessionBinding, Player, SyncLoadoutPacket, OwnerCommitResult> commitP04Transaction)
  {
    ArgumentNullException.ThrowIfNull(commitP04Transaction);
    return HandleLoadoutCore(envelope, packet, null, commitP04Transaction, null);
  }

  private PlayerLoadoutCommit HandleLoadoutCore(
      InboundPacketEnvelope envelope,
      SyncLoadoutPacket packet,
      Func<SessionBinding, Player, SyncLoadoutPacket, IPlayerLoadoutTransaction>? prepare,
      Func<SessionBinding, Player, SyncLoadoutPacket, OwnerCommitResult>? legacyCommit,
      Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput) {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    byte loadoutIndex = packet.LoadoutIndex;
    ushort visibility = packet.AccessoryVisibilityMask;
    PlayerLoadoutCommit? mutation = null;
    OwnerCommitResult gate = _sessions.ExecuteAuthenticatedPlayerPacket(
      envelope,
      SessionAdmissionCoordinator.SyncLoadoutMessageId,
      [
        SessionStage.AwaitPlayerData,
        SessionStage.AwaitSectionRequest,
        SessionStage.Synchronizing,
        SessionStage.Active
      ],
      binding =>
      {
        if (!_players.TryGetForBinding(binding, out Player? player) || player is null)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.StaleEntity,
            "Player identity is no longer active.");
        }

        OwnerCommitResult validation = _players.ValidateLoadoutProjection(player, loadoutIndex);
        if (validation.Status != OwnerCommitStatus.Applied)
        {
          return validation;
        }

        var normalized = new SyncLoadoutPacket
        {
          Player = binding.PlayerSlot,
          LoadoutIndex = loadoutIndex,
          AccessoryVisibilityMask = visibility
        };

        IPlayerLoadoutTransaction? prepared = null;
        if (prepare is not null) {
          prepared = prepare.Invoke(binding, player, normalized)
            ?? throw new InvalidOperationException("P04 returned no prepared loadout transaction.");
          OwnerCommitResult inventoryValidation = prepared.Validate();
          if (inventoryValidation.Status != OwnerCommitStatus.Applied) {
            return inventoryValidation;
          }
        }

        // Finish all P02 allocation and validation before P04 first changes equipment.
        CommitVersion version = _players.ReserveCommitVersion();
        var recipients = _sessions.CaptureActiveRecipients(envelope.Epoch);
        var committed = new PlayerLoadoutCommit(
          OwnerCommitResult.Applied(version), loadoutIndex, visibility) {
          Projections = recipients.Count == 0 ? [] : Array.AsReadOnly(new[] {
            new SessionPacketProjection(SessionAdmissionCoordinator.SyncLoadoutMessageId,
              new SyncLoadoutPacket {
                Player = binding.PlayerSlot,
                LoadoutIndex = loadoutIndex,
                AccessoryVisibilityMask = visibility
              }, recipients, envelope.Epoch, version)
          })
        };

        OwnerCommitResult? outputValidation = prepareOutput?.Invoke(committed.Projections);
        if (outputValidation is not null && outputValidation.Status != OwnerCommitStatus.Applied) {
          return outputValidation;
        }

        if (prepared is not null) {
          prepared.Commit();
        } else {
          OwnerCommitResult inventoryResult = legacyCommit!.Invoke(binding, player, normalized);
          if (inventoryResult.Status != OwnerCommitStatus.Applied) {
            return inventoryResult;
          }
        }

        // Both owners commit in one exclusive loop window. This final assignment cannot reject.
        _players.CommitPreparedLoadoutProjection(player, loadoutIndex, visibility, version);
        mutation = committed;
        return mutation.Result;
      });

    return mutation ?? new PlayerLoadoutCommit(
      gate,
      loadoutIndex,
      visibility);
  }

  public PlayerPacketMutationResult HandleControls(
    InboundPacketEnvelope envelope,
    PlayerControlsPacket packet,
    bool preserveServerPosition,
    Func<byte, bool> isSelectedItemValid,
    IPlayerRuntimeWorldCaptureSource? runtimeWorldCapture = null,
    PlayerRuntimeFactsCaptureService? runtimeFactsCapture = null,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _players.ApplyControls(
      envelope,
      packet,
      _sessions,
      preserveServerPosition,
      isSelectedItemValid,
      runtimeWorldCapture,
      runtimeFactsCapture,
      prepareOutput);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyTogglePvp"/>
  public PlayerPacketMutationResult HandleTogglePvp(
    InboundPacketEnvelope envelope,
    TogglePVPPacket packet,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _players.ApplyTogglePvp(envelope, packet, _sessions, prepareOutput);
  }

  public PlayerPacketMutationResult HandleTeamChange(
    InboundPacketEnvelope envelope,
    TeamChangePacket packet)
  {
    return _players.ApplyTeamChange(
      envelope,
      packet,
      SessionAdmissionCoordinator.TeamChangeMessageId,
      _sessions);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyBuffUpload"/>
  public PlayerPacketMutationResult HandleBuffs(
    InboundPacketEnvelope envelope,
    PlayerBuffsPacket packet,
    Func<ushort, bool> isValidBuff,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _players.ApplyBuffUpload(envelope, packet, _sessions, isValidBuff, prepareOutput);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyZoneUpload"/>
  public PlayerPacketMutationResult HandleZones(
    InboundPacketEnvelope envelope,
    SyncPlayerZonePacket packet,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _players.ApplyZoneUpload(envelope, packet, _sessions, prepareOutput);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyTilePickingRelay"/>
  public PlayerPacketMutationResult HandleTilePicking(
    InboundPacketEnvelope envelope,
    SyncTilePickingPacket packet,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    return _players.ApplyTilePickingRelay(envelope, packet, _sessions, prepareOutput);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyStealthUpload"/>
  public PlayerPacketMutationResult HandleStealth(
    InboundPacketEnvelope envelope,
    PlayerStealthPacket packet,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _players.ApplyStealthUpload(envelope, packet, _sessions, prepareOutput);
  }

  public PlayerPacketMutationResult HandleLuck(
    InboundPacketEnvelope envelope,
    UpdatePlayerLuckFactorsPacket packet,
    Func<Player, PlayerLuckCalculationContext> captureCalculationContext)
  {
    return _players.ApplyLuckFactorUpload(
      envelope, packet, _sessions, captureCalculationContext);
  }

  public PlayerPacketMutationResult HandleLuck(
    InboundPacketEnvelope envelope,
    UpdatePlayerLuckFactorsPacket packet,
    PlayerLuckCalculationContext calculationContext)
  {
    return _players.ApplyLuckFactorUpload(
      envelope,
      packet,
      _sessions,
      calculationContext);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyMinionRestTarget"/>
  public PlayerPacketMutationResult HandleMinionRestTarget(
    InboundPacketEnvelope envelope,
    MinionRestTargetUpdatePacket packet,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    return _players.ApplyMinionRestTarget(envelope, packet, _sessions, prepareOutput);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyMinionAttackTarget"/>
  public PlayerPacketMutationResult HandleMinionAttackTarget(
    InboundPacketEnvelope envelope,
    MinionAttackTargetUpdatePacket packet,
    Func<short, EntityHandle?> resolveActiveNpcHandle,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    return _players.ApplyMinionAttackTarget(
      envelope,
      packet,
      _sessions,
      resolveActiveNpcHandle,
      prepareOutput);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyHealEffect"/>
  public PlayerPacketMutationResult HandleHealEffect(
    InboundPacketEnvelope envelope,
    PlayerHealPacket packet,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    return _players.ApplyHealEffect(envelope, packet, _sessions, prepareOutput);
  }

  /// <inheritdoc cref="PlayerOwner.ApplyHealToPlayer"/>
  public PlayerPacketMutationResult HandleHealToPlayer(
    InboundPacketEnvelope envelope,
    Unknown66Packet packet,
    Func<Player, int> effectiveMaximumLife,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null) {
    return _players.ApplyHealToPlayer(
      envelope,
      packet,
      _sessions,
      effectiveMaximumLife,
      prepareOutput);
  }

  /// <summary>
  /// The optional synchronous preparation callback may encode and capture output before commit.
  /// It must not mutate inputs or owners, perform I/O, or reenter player/session operations.
  /// Submit the captured output only when the returned lifecycle result is Applied.
  /// </summary>
  public PlayerLifecycleResult HandleSpawn(
    InboundPacketEnvelope envelope,
    PlayerSpawnPacket packet,
    bool sendGameplayHostProjection,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null,
    PacketVector2? fallbackSpawnPosition = null)
  {
    return _lifecycle.ApplySpawn(envelope, packet, sendGameplayHostProjection,
        prepareOutput, fallbackSpawnPosition);
  }

  /// <inheritdoc cref="PlayerLifecycleCoordinator.ApplyDeath"/>
  public PlayerLifecycleResult HandleDeath(
    InboundPacketEnvelope envelope,
    PlayerDeathV2Packet packet,
    int respawnTimer,
    bool pvpDeath,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    return _lifecycle.ApplyDeath(envelope, packet, respawnTimer, pvpDeath, prepareOutput);
  }
}
