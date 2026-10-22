using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.Session;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private string? HandleHealEffect(
      InboundPacketEnvelope envelope,
      PlayerHealPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleHealEffect(
      envelope, packet, prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.PlayerHealMessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player heal effect was rejected.";
  }

  private string? HandleHealToPlayer(
      InboundPacketEnvelope envelope,
      Unknown66Packet packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleHealToPlayer(
      envelope, packet, static player => player.Vitals.GetEffectiveMaximumLife(player.Buffs),
      prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.Unknown66MessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The target heal was rejected.";
  }

  private string? HandleTilePicking(
      InboundPacketEnvelope envelope,
      SyncTilePickingPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleTilePicking(
      envelope, packet, prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.SyncTilePickingMessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The tile-picking relay was rejected.";
  }

  private string? HandlePvpUpload(
      InboundPacketEnvelope envelope,
      TogglePVPPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<SessionEpoch> notificationRecipients = Array.Empty<SessionEpoch>();
    Version4EncodedPacket? notification = null;
    OwnerCommitResult PreparePvpOutput(IReadOnlyList<SessionPacketProjection> projections) {
      OwnerCommitResult encodedResult = CaptureSessionOutputIntents(
        SessionAdmissionCoordinator.TogglePvpMessageId, projections,
        out IReadOnlyList<OutboundPacketIntent> encoded);
      if (encodedResult.Status != OwnerCommitStatus.Applied) {
        return encodedResult;
      }

      if (!_composition.Players.TryGetBoundPlayer(
          envelope.Epoch, _composition.SessionAdmission, out var player) || player is null) {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.StaleEntity,
          "The PvP notification player identity is no longer active.");
      }

      try {
        PacketRgb color = GetTeamColor(player.Team.Current);
        notification = _composition.Version4WireProfile.Encode(
          WireDirection.ServerToClient, new NetModulesPacket {
            ModuleId = (ushort)Packet82ModuleId.Text,
            Data = new Packet82TextData(
              byte.MaxValue,
              NetworkText.Key(packet.Hostile ? "LegacyMultiplayer.11" : "LegacyMultiplayer.12",
                NetworkText.Literal(player.Appearance.Name)),
              color.Red, color.Green, color.Blue)
          });
        notificationRecipients = _composition.SessionAdmission.CaptureActiveSessionEpochs();
        captured = encoded;
        return OwnerCommitResult.Applied(CommitVersion.Initial);
      } catch (Exception exception) when (exception is ArgumentException or
          InvalidDataException or InvalidOperationException or NotSupportedException) {
        return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
          $"Packet 30 notification preparation failed: {exception.Message}");
      }
    }

    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleTogglePvp(
      envelope, packet, prepareOutput: PreparePvpOutput);
    intents = Array.Empty<OutboundPacketIntent>();
    if (committed.Result.Status != OwnerCommitStatus.Applied) {
      return committed.Result.Detail ?? "The player PvP upload was rejected.";
    }

    // Bytes and recipients are prepared before commit; stamp them with its actual version.
    var output = new List<OutboundPacketIntent>(captured);
    foreach (SessionEpoch target in notificationRecipients) {
      output.Add(new OutboundPacketIntent(
        [target], null, notification!.MessageId, notification.Payload,
        committed.Result.Version!.Value));
    }

    intents = output.AsReadOnly();
    return null;
  }

  private string? HandleBuffUpload(
      InboundPacketEnvelope envelope,
      PlayerBuffsPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleBuffs(
      envelope, packet, Version4PlayerBuffCatalog.IsValidBuff, prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.PlayerBuffsMessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player buff upload was rejected.";
  }

  private string? HandleDeathUpload(
      InboundPacketEnvelope envelope,
      PlayerDeathV2Packet packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    bool pvpDeath = (packet.DeathFlags & 1) != 0;
    PlayerLifecycleResult committed = _composition.PlayerStatePacketHandlers.HandleDeath(
      envelope,
      packet,
      respawnTimer: 0,
      pvpDeath,
      prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.PlayerDeathMessageId,
          projections,
          out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player death upload was rejected.";
  }

  private string? HandleZoneUpload(
      InboundPacketEnvelope envelope,
      SyncPlayerZonePacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleZones(
      envelope, packet, prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.SyncPlayerZoneMessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player zone upload was rejected.";
  }

  private string? HandleStealthUpload(
      InboundPacketEnvelope envelope,
      PlayerStealthPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleStealth(
      envelope, packet, prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.PlayerStealthMessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player stealth upload was rejected.";
  }

  private string? HandleMinionRestTargetUpload(
      InboundPacketEnvelope envelope,
      MinionRestTargetUpdatePacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed =
      _composition.PlayerStatePacketHandlers.HandleMinionRestTarget(
        envelope, packet, prepareOutput: projections => {
          return CaptureSessionOutputIntents(
            SessionAdmissionCoordinator.MinionRestTargetMessageId, projections, out captured);
        });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The minion rest-target upload was rejected.";
  }

  private string? HandleMinionAttackTargetUpload(
      InboundPacketEnvelope envelope,
      MinionAttackTargetUpdatePacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed =
      _composition.PlayerStatePacketHandlers.HandleMinionAttackTarget(
        envelope, packet, _composition.ResolveActiveNpcHandle, prepareOutput: projections => {
          return CaptureSessionOutputIntents(
            SessionAdmissionCoordinator.MinionAttackTargetMessageId, projections, out captured);
        });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The minion attack-target upload was rejected.";
  }

  private string? HandleControlsUpload(
      InboundPacketEnvelope envelope,
      PlayerControlsPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleControls(
      envelope, packet,
      preserveServerPosition: false,
      // Version4 Player.inventory includes the mouse item at slot 58.
      isSelectedItemValid: static selected => selected <= InventorySlotLayout.Mouse,
      runtimeWorldCapture: _composition.PlayerControlsWorldCapture,
      prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.PlayerControlsMessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player controls upload was rejected.";
  }

  private string? HandleAppearanceUpload(
      InboundPacketEnvelope envelope,
      SyncPlayerPacket packet,
      bool isJourneyWorld,
      out IReadOnlyList<OutboundPacketIntent> intents,
      out bool closeAfterResponse) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleAppearance(
      envelope, packet,
      skinVariantCount: 8,
      maximumNameLength: PlayerRules.DefaultNameLengthLimit,
      isJourneyWorld: isJourneyWorld,
      checkDuplicateName: true,
      prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.SyncPlayerMessageId, projections, out captured);
      });
    bool isKickRejection = committed.Result.Status == OwnerCommitStatus.Rejected &&
      committed.CloseAfterResponse &&
      committed.ResponseMessageId == SessionAdmissionCoordinator.KickMessageId &&
      committed.ResponsePacket is KickPacket;
    intents = committed.Result.Status == OwnerCommitStatus.Applied || isKickRejection
      ? captured : Array.Empty<OutboundPacketIntent>();
    closeAfterResponse = isKickRejection;
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player profile upload was rejected.";
  }

  private string? HandleLifeUpload(
      InboundPacketEnvelope envelope,
      PlayerLifeManaPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerPacketMutationResult committed = _composition.PlayerStatePacketHandlers.HandleLifeMana(
      envelope, packet, prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.PlayerLifeManaMessageId, projections, out captured);
      });
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player life upload was rejected.";
  }

  private string? HandleSpawn(
      InboundPacketEnvelope envelope,
      PlayerSpawnPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    PacketVector2? fallbackSpawnPosition = null;
    if (packet.SpawnX == -1 && packet.SpawnY == -1 &&
        _composition.SessionBindings.TryGetByEpoch(envelope.Epoch, out var binding) &&
        binding is not null &&
        _composition.Players.TryGetForBinding(binding, out var player) && player is not null) {
      var header = _composition.WorldOwner.CaptureWorldHeader().Header;
      var spawn = header.Spawn;
      var teams = header.TeamSpawns;
      if (teams.IsReady && teams.TeamBasedSpawns && packet.Team != 0 &&
          packet.Team < teams.ExtraSpawnPoints.Count) {
        spawn = teams.ExtraSpawnPoints[packet.Team];
      }
      // Packet 12 keeps the personal-spawn sentinel on the wire. Resolve the actual
      // world/team position before activating AI; packet 13 may arrive a tick later.
      fallbackSpawnPosition = new PacketVector2(
          spawn.X * 16 + 8 - player.RuntimeSpatial.Width / 2,
          spawn.Y * 16 - player.RuntimeSpatial.Height);
    }
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerLifecycleResult committed = _composition.PlayerStatePacketHandlers.HandleSpawn(
      envelope, packet, sendGameplayHostProjection: true, prepareOutput: projections => {
        return CaptureSessionOutputIntents(
          SessionAdmissionCoordinator.PlayerSpawnMessageId, projections, out captured);
      }, fallbackSpawnPosition: fallbackSpawnPosition);
    intents = committed.Result.Status == OwnerCommitStatus.Applied
      ? captured : Array.Empty<OutboundPacketIntent>();
    return committed.Result.Status == OwnerCommitStatus.Applied
      ? null : committed.Result.Detail ?? "The player spawn was rejected.";
  }

  private OwnerCommitResult CaptureSessionOutputIntents(
      byte sourceMessageId,
      IReadOnlyList<SessionPacketProjection> projections,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    intents = Array.Empty<OutboundPacketIntent>();
    try {
      IReadOnlyList<OutboundPacketIntent> encoded =
        _composition.SessionPacketHandlers.CreateIntents(
          projections, _composition.Version4WireProfile);
      var captured = new List<OutboundPacketIntent>();
      foreach (OutboundPacketIntent intent in encoded) {
        foreach (SessionEpoch target in intent.TargetEpochs) {
          if (target != intent.IgnoreEpoch) {
            captured.Add(new OutboundPacketIntent(
              [target], null, intent.MessageId, intent.Payload, intent.CommitVersion));
          }
        }
      }

      intents = captured.AsReadOnly();
      return OwnerCommitResult.Applied(CommitVersion.Initial);
    } catch (Exception exception) when (exception is ArgumentException or
        InvalidDataException or InvalidOperationException or NotSupportedException) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
        $"Packet {sourceMessageId} output preparation failed: {exception.Message}");
    }
  }

  private async ValueTask<string?> SendCommittedSessionOutputAsync(
      byte sourceMessageId,
      IReadOnlyList<OutboundPacketIntent> intents,
      CancellationToken cancellationToken,
      ISet<SessionEpoch>? failedTargets = null) {
    var failures = new List<string>();
    // Callers may inspect returned submission failures to gate their remaining output.
    failedTargets ??= new HashSet<SessionEpoch>();
    foreach (OutboundPacketIntent intent in intents) {
      SessionEpoch target = intent.TargetEpochs.Single();
      if (intent.MessageId == SessionAdmissionCoordinator.FinishedConnectingMessageId &&
          failedTargets.Contains(target)) {
        failures.Add($"packet {intent.MessageId}, version {intent.CommitVersion.Value}, " +
          $"target {target.Value}: NotSubmitted after earlier projection failure");
        continue;
      }

      IReadOnlyList<SendReceipt> receipts = await SendWithReceiptsAsync(
        intent, cancellationToken).ConfigureAwait(false);
      if (!IsSuccessfulSubmission(receipts, target)) {
        failedTargets.Add(target);
        string outcome = receipts.Count == 0 ? "NotSubmitted" : string.Join(", ",
          receipts.Select(receipt => $"{receipt.Epoch.Value}:{receipt.Certainty}"));
        failures.Add($"packet {intent.MessageId}, version {intent.CommitVersion.Value}, " +
          $"target {target.Value}: {outcome}");
      }
    }

    // Submission failure does not undo committed state or authorize replay of captured output.
    return failures.Count == 0 ? null :
      $"Packet {sourceMessageId} committed, but projection submission failed " +
      $"({string.Join("; ", failures)}).";
  }
}
