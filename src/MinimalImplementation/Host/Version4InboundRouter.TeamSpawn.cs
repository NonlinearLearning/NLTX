using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

public sealed partial class Version4InboundRouter {
  private sealed record PendingTeamSpawn(
    WorldSectionSendBatch Sections,
    IReadOnlyCollection<WorldSectionCoordinate> RequiredSections,
    OutboundPacketIntent Acknowledgement);

  private string? HandleTeamChange(
      InboundPacketEnvelope envelope,
      TeamChangePacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    intents = Array.Empty<OutboundPacketIntent>();
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    PlayerTeamChangeCommit committed = _composition.PlayerTeamChangeCoordinator.HandleTeamChange(
      envelope, packet, proposed => {
        return CaptureTeamChangeOutput(
          SessionAdmissionCoordinator.TeamChangeMessageId, proposed, out captured);
      });
    if (committed.Result.Status != OwnerCommitStatus.Applied) {
      return committed.Result.Detail ?? "Team change was rejected.";
    }

    intents = captured;
    return null;
  }

  private string? HandleTeamChangeFromUi(
      InboundPacketEnvelope envelope,
      TeamChangeFromUIPacket packet,
      out IReadOnlyList<OutboundPacketIntent> intents,
      out PendingTeamSpawn? pending,
      out bool mustClose) {
    intents = Array.Empty<OutboundPacketIntent>();
    pending = null;
    mustClose = false;
    if (_worldSectionRequests is null) {
      return "Packet 157 is NotReady: authoritative section attachment capture is required.";
    }

    WorldSectionRequestCoordinator.PreparedTeamSpawn? prepared = null;
    PendingTeamSpawn? preparedPending = null;
    IReadOnlyList<OutboundPacketIntent> captured = Array.Empty<OutboundPacketIntent>();
    try {
      OwnerCommitResult PrepareTeamOutput(PlayerTeamChangeCommit proposed) {
        if (prepared is null) {
          return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
            "Packet 157 section preparation did not produce an arrangement.");
        }

        OwnerCommitResult encodedResult = CaptureTeamChangeOutput(
          SessionAdmissionCoordinator.TeamChangeFromUiMessageId, proposed,
          out IReadOnlyList<OutboundPacketIntent> encoded);
        if (encodedResult.Status != OwnerCommitStatus.Applied) {
          return encodedResult;
        }

        // Section bytes are already encoded. Prepare the complete 158 barrier before commit.
        if (prepared.RequiresExtraSpawn) {
          OutboundPacketIntent acknowledgement = encoded.Single(intent => intent.MessageId == 158);
          preparedPending = new PendingTeamSpawn(
            prepared.Batch, prepared.RequiredSections, acknowledgement);
        }

        captured = encoded.Where(intent => intent.MessageId != 158).ToArray();
        return OwnerCommitResult.Applied(CommitVersion.Initial);
      }

      PlayerTeamChangeCommit committed =
        _composition.PlayerTeamChangeCoordinator.HandleTeamChangeFromUi(
          envelope, packet, (binding, team) => {
            prepared = _worldSectionRequests.PrepareTeamSpawn(binding, team);
            return prepared;
          }, PrepareTeamOutput);
      if (committed.Result.Status != OwnerCommitStatus.Applied || prepared is null) {
        return committed.Result.Detail ?? "The UI team change was rejected.";
      }

      intents = captured;
      pending = preparedPending;
      return null;
    } catch (Exception exception) when (exception is ArgumentException or
        InvalidDataException or InvalidOperationException or NotSupportedException) {
      if (prepared?.IsCommitted == true) {
        _worldSectionRequests.RetireTransfer(prepared.Batch.Token);
        mustClose = true;
      }

      return $"Packet 157 team-spawn preparation was rejected: {exception.Message}";
    }
  }

  private OwnerCommitResult CaptureTeamChangeOutput(
      byte sourceMessageId,
      PlayerTeamChangeCommit proposed,
      out IReadOnlyList<OutboundPacketIntent> intents) {
    intents = Array.Empty<OutboundPacketIntent>();
    try {
      SessionPacketProjection[] notifications = proposed.Projections
        .Where(projection => projection.MessageId is not (10 or 11)).ToArray();
      OwnerCommitResult encodedResult = CaptureSessionOutputIntents(
        sourceMessageId, notifications, out IReadOnlyList<OutboundPacketIntent> encoded);
      if (encodedResult.Status != OwnerCommitStatus.Applied) {
        return encodedResult;
      }

      var captured = new List<OutboundPacketIntent>(encoded);
      OutboundPacketIntent notification = CreateTeamChangeNotification(proposed);
      foreach (SessionEpoch target in notification.TargetEpochs) {
        captured.Add(new OutboundPacketIntent(
          [target], null, notification.MessageId, notification.Payload, notification.CommitVersion));
      }

      intents = captured.AsReadOnly();
      return OwnerCommitResult.Applied(CommitVersion.Initial);
    } catch (Exception exception) when (exception is ArgumentException or
        InvalidDataException or InvalidOperationException or NotSupportedException) {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady,
        $"Packet {sourceMessageId} output preparation failed: {exception.Message}");
    }
  }

  private OutboundPacketIntent CreateTeamChangeNotification(PlayerTeamChangeCommit committed) {
    int messageIndex = committed.Team.Current == PlayerTeam.MaximumTeam
      ? 22 : 13 + committed.Team.Current;
    PacketRgb color = GetTeamColor(committed.Team.Current);
    return _composition.Packet82IntentProducer.CreateIntent(
      (ushort)Packet82ModuleId.Text,
      new Packet82TextData(
        byte.MaxValue,
        NetworkText.Key($"LegacyMultiplayer.{messageIndex}",
          NetworkText.Literal(committed.PlayerName)),
        color.Red, color.Green, color.Blue),
      committed.NotificationRecipients,
      committed.Result.Version!.Value);
  }

  private static PacketRgb GetTeamColor(byte team) {
    return team switch {
      0 => new(255, 255, 255),
      1 => new(218, 59, 59),
      2 => new(59, 218, 85),
      3 => new(59, 149, 218),
      4 => new(242, 221, 100),
      5 => new(224, 100, 242),
      _ => throw new InvalidOperationException("The player team is invalid.")
    };
  }

  private async ValueTask<bool> SendTeamSpawnAsync(
      PendingTeamSpawn pending, CancellationToken cancellationToken) {
    bool completed = false;
    try {
      if (!await SendSectionBatchAsync(
          pending.Sections, SessionStage.Active, cancellationToken).ConfigureAwait(false)) {
        return false;
      }

      bool canAcknowledge = false;
      await _composition.CommitLoop.SubmitAsync(() => {
        canAcknowledge = CanSendSections(pending.Sections, SessionStage.Active) &&
          _worldSectionRequests!.HasQueuedSections(
            pending.Sections.Token, pending.RequiredSections);
      }, cancellationToken).ConfigureAwait(false);
      if (!canAcknowledge ||
          !await SendJoinPacketAsync(pending.Acknowledgement, cancellationToken)
            .ConfigureAwait(false)) {
        return false;
      }

      completed = true;
      return true;
    } finally {
      if (!completed) {
        await RetireSectionTransferAsync(pending.Sections).ConfigureAwait(false);
      }
    }
  }
}
